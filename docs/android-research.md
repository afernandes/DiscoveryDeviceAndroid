# Investigação do pareamento Wi-Fi do Android Studio

Data: 26/10/2026. Identificadores de dispositivo e endereços dos exemplos foram anonimizados. Inspeção estática, sem executar código do Android Studio nem efetuar pareamento em aparelho.

## Identidade do artefato

- Android Studio instalado: `C:\Program Files\Android\Android Studio`.
- `build.txt`: `AI-262.9437.185.2621.16467767`.
- Artefato: `plugins\android\lib\android.jar`.
- SHA-256 retornado por REA `open_binary`: `8d51f4dbaf3af51493c5f01aeb0dc0caa5365a1e91bdb21fbca24006443e3e57`.
- REA classificou o JAR como `format: zip`, `kind: archive`, arquitetura não aplicável. Esse retorno de abertura não forneceu `evidence_id`; nenhum ID foi inventado. A sessão foi fechada com `close_binary` ao final.
- A análise de bytecode usou o `javap.exe` já incluído em `jbr\bin`, sem instalar ferramentas. A leitura do ZIP usou `jar.exe tf` do mesmo JBR.

## Observações estáticas do produto instalado

Os pontos abaixo se baseiam em instruções de bytecode, constantes e chamadas, não apenas em nomes de arquivos.

1. `com.android.tools.idea.adb.wireless.WiFiPairingServiceImpl` guarda o prefixo `studio-`. `checkMdnsSupport` monta os argumentos `mdns` e `check`; distingue comando desconhecido, mDNS desativado e resultado suportado. O serviço oferece tanto varredura pontual quanto acompanhamento de serviços.
2. `WiFiPairingServiceImplKt.createPairingString`, tabela `BootstrapMethods` observada por `javap -p -v`, contém a receita de concatenação `WIFI:T:ADB;S:\u0001;P:\u0001;;`. Os dois parâmetros são o nome de instância e a senha, nessa ordem.
3. `createRandomInstanceName` solicita 10 caracteres do alfabeto `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-`. `createRandomPassword` solicita 12 caracteres de um alfabeto que também contém símbolos. Usar somente caracteres alfanuméricos criptograficamente aleatórios na reconstrução evita regras de escape e continua respeitando o formato; essa é uma decisão da reconstrução.
4. `WiFiPairingServiceImpl$generateQrCode$2.invokeSuspend`, offsets 32–98: obtém prefixo e instância aleatória, gera senha, cria a string, chama `QrCodeGenerator.encodeQrCodeToImage`, e constrói `QrCodeImage` conservando nome, senha, conteúdo e imagem.
5. `QrCodeScanningController$MyModelListener.qrCodeServicesDiscovered`, offsets 216–264: compara `PairingMdnsService.getServiceName()` a `QrCodeImage.getServiceName()` via `Intrinsics.areEqual`. Somente a instância correspondente inicia `startPairingDevice` com a senha daquele QR. Portanto, não se deve parear o primeiro anúncio arbitrário encontrado na rede.
6. `QrCodeScanningController$startMdnsTrackingService$1$1` lê nome de instância, IPv4, porta e serial de `MdnsTrackServiceInfo` para formar os serviços disponíveis ao modelo.
7. `AdbServiceWrapperAdbLibImpl.mdnsPair` delega a `AdbHostServices.pair(DeviceAddress, String, Continuation)`. A criptografia e a troca de chaves pertencem ao ADB. A UI orquestra esse processo.
8. `DeviceNeedsUpdateBanner` contém o aviso de capacidade limitada do ADB Wi-Fi v1.0 e um link para `https://d.android.com/r/studio-ui/wifi-pairing`. A presença desse aviso não prova impossibilidade de parear dispositivos v1.0. A UI das imagens oferecia os dois métodos mesmo mostrando o aviso.

Comandos de reprodução da inspeção (substituir `$cp` e `$jp` pelos caminhos acima):

```powershell
& $jp -classpath $cp -c -p com.android.tools.idea.adb.wireless.WiFiPairingServiceImpl
& $jp -classpath $cp -c -p com.android.tools.idea.adb.wireless.WiFiPairingServiceImplKt
& $jp -classpath $cp -p -v com.android.tools.idea.adb.wireless.WiFiPairingServiceImplKt
& $jp -classpath $cp -c -p 'com.android.tools.idea.adb.wireless.WiFiPairingServiceImpl$generateQrCode$2'
& $jp -classpath $cp -c -p 'com.android.tools.idea.adb.wireless.QrCodeScanningController$MyModelListener'
& $jp -classpath $cp -c -p com.android.tools.idea.adb.wireless.AdbServiceWrapperAdbLibImpl
& $jp -classpath $cp -c -p com.android.tools.idea.adb.wireless.DeviceNeedsUpdateBanner
```

## Protocolo e portas: corroboração AOSP

A [arquitetura oficial do ADB Wi-Fi](https://android.googlesource.com/platform/packages/modules/adb/+/HEAD/docs/dev/adb_wifi.md) distingue `_adb-tls-pairing._tcp` e `_adb-tls-connect._tcp`. O primeiro anuncia o servidor de pareamento; o segundo oferece o transporte TLS para depuração. São portas dinâmicas diferentes. A UI por código usa o segredo de seis dígitos exibido pelo aparelho. Para QR, o computador gera nome de instância e segredo, o aparelho lê o QR e anuncia o servidor de pareamento com esse nome. O servidor ADB pode conectar automaticamente depois do pareamento. A alternativa explícita deve usar o endpoint de conexão, nunca reaproveitar a porta do pareamento. O serviço `_adb._tcp` pertence ao fluxo legado; não é equivalente ao transporte TLS.

Esses fatos sustentam uma extensão que reutiliza o ADB instalado, consulta os anúncios e apresenta o fluxo ao usuário. Não é necessário reimplementar TLS nem distribuir um servidor Android.

## Descoberta moderna e ADB Wi-Fi 2.0

A [documentação atual oficial do ADB](https://developer.android.com/tools/adb) usa `adb mdns track-services --proto-text` para diagnósticos. O exemplo inclui `product_model`, `build_version_sdk_full`, `given_name`, `serial`, `mdns_service_version`, IPv4, IPv6 e porta. O documento informa suporte a ADB Wi-Fi 2.0 no Android 17 ou superior e recomenda verificar o valor anunciado de `mdns_service_version`.

A reconstrução deve exibir versão desconhecida quando não há metadados; o texto tabular antigo de `adb mdns services` não prova modelo/API/versão. Ter Android 11+ permite o pareamento Wi-Fi original; isso não implica suporte a 2.0. O diagnóstico de mDNS vazio deve apontar possíveis problemas de rede e depuração desativada, sem afirmar uma causa única.

## Contratos recomendados para reconstrução

- QR: `WIFI:T:ADB;S:studio-<instancia-aleatoria>;P:<segredo-aleatorio>;;`. Fazer match exato do nome da sessão; gerar novo segredo para nova sessão e descartar o anterior ao cancelar/fechar.
- Código: validar exatamente seis dígitos ASCII, preservando zeros à esquerda. A porta exibida pela opção de pareamento por código pertence a `adb pair`.
- Segurança do processo: passar argumentos separadamente, sem shell. Preferir stdin para o segredo; não o gravar em logs, histórico ou configuração persistente.
- Pós-pareamento: primeiro aguardar o ADB apresentar o dispositivo online; caso necessário, conectar ao anúncio `_adb-tls-connect._tcp` correspondente. Ao usar IP para correlação, exigir correspondência sem ambiguidade. Não percorrer outros dispositivos da rede tentando conectar automaticamente.
- Conexão manual: pedir endereço/porta da tela principal de depuração sem fio. Não deduzir a porta 5555 nem converter automaticamente o fluxo para `adb tcpip`.
- Estado: separar descoberto, aguardando código/QR, pareando, pareado aguardando conexão e conectado. Sucesso de `adb pair` sozinho não comprova disponibilidade para depuração.
- Ciclo de vida: cancelar polling/processos próprios ao fechar a janela; não executar `adb kill-server`, pois o servidor pode ser compartilhado pelo Visual Studio e Android Studio.
- Identidade: não atribuir modelo ou versão pelo prefixo da instância. Enriquecer apenas com metadados anunciados ou propriedades consultadas de um dispositivo já autorizado e online.

## Cobertura e lacunas

| Questão | Resultado | Evidência e limite |
| --- | --- | --- |
| Payload QR | Respondida | Bytecode instalado + AOSP; não foi realizado scan em aparelho |
| Correlação QR/anúncio | Respondida | Comparação exata observada no bytecode |
| Portas de pareamento/conexão | Respondida | Arquitetura oficial; nenhuma porta real foi assumida |
| Descoberta com metadados | Respondida parcialmente | Bytecode consome campos + documentação CLI; não foi capturado anúncio de aparelho nesta investigação |
| Aviso v1.0 | Respondida parcialmente | Aviso estático confirmado; todos os comportamentos novos de v2.0 não foram decompilados |
| Depuração pelo Visual Studio após conexão | Não verificada nesta investigação | Exige execução da extensão e projeto Android/aparelho |
| Identidade visual | Referência disponível | As cinco imagens descrevem a UX; não equivalem a execução do produto |

Nenhum código proprietário foi copiado para a implementação. A reconstrução deve usar os contratos públicos de ADB, com implementação própria.

## Verificação passiva do ADB local (complemento)

ADB encontrado em `C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe`:

```text
Android Debug Bridge version 1.0.41
Version 37.0.1-15733141
```

`adb mdns services --proto-text` e `--proto-binary` foram rejeitados com `mdns services doesn't take any arguments`. `adb help` não lista `track-services`, mas `adb mdns track-services --proto-text` funcionou nesta instalação. Portanto, detectar suporte pela execução/protocolo é mais robusto que depender da descrição em help.

O cliente fica em execução acompanhando mudanças. A captura passiva abaixo foi obtida em uma janela de cinco segundos; somente o processo cliente próprio foi encerrado no timeout. Não foram enviados `pair`, `connect`, `kill-server` nem comandos de dispositivo.

```text

Services:
tls {
  service {
    instance: "adb-EXEMPLO1234-demo"
    service: "_adb-tls-connect._tcp"
    ipv4: "192.0.2.10"
    ipv6: "2001:db8::10"
    ipv6: "fe80::10"
    ipv6: "2001:db8::11"
    ipv6: "2001:db8::12"
    port: 39775
    product_model: "SM-X820"
    build_version_sdk_full: "36.1"
    mdns_service_version: "1"
    hostname: "Android-2.local"
  }
  known_device: true
}

```

Esse anúncio comprova que é possível obter o modelo SM-X820, API 36.1 e versão mDNS 1 sem abrir uma shell no aparelho e sem primeiro pareá-lo nesta investigação. `known_device: true` foi reportado pelo ADB; não se investigou quando ou por quem ocorreu esse pareamento prévio. `serial` está ausente. Os IPv6 são campos repetidos. A porta de conexão coincide com a imagem fornecida, mas continua sendo valor dinâmico, nunca constante de implementação.

Framing observado: linha vazia inicial, `Services:` e conteúdo protobuf textual com chaves aninhadas, encerrado por CRLF após a chave final; uma linha vazia adicional não é garantida; stderr vazio. Não houve mudança de rede durante a captura, portanto não foi observado um segundo snapshot. Ao implementar leitura pontual, não aguardar EOF de um cliente contínuo: impor timeout/cancelamento, aceitar apenas snapshots completos e encerrar somente o processo próprio. Uma captura interrompida no meio de um bloco não deve substituir o estado válido anterior.

O [schema público do adblib](https://android.googlesource.com/platform/tools/base/+/ff1751d7365def37a013f92f574f999e50c81359/adblib/proto/mdns.proto) prevê coleções `tcp`, `tls` e `pair`, cada uma contendo `service`; `known_device` pertence ao invólucro `tls`. Há divergência de nome do modelo nesse snapshot do schema (`device_model`) em relação à saída local (`product_model`); para compatibilidade, aceitar ambos, priorizando o campo efetivamente anunciado. Campos desconhecidos devem ser tolerados. Não confundir o campo textual `service: "_adb-tls-connect._tcp"` com o bloco `service { ... }`.
