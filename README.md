# Android via Wi-Fi para Visual Studio

Extensão em C# para descoberta, pareamento e conexão de dispositivos Android pela rede sem fio. A interface usa uma janela do Visual Studio com Remote UI, temas do IDE, navegação por teclado e textos em português.

## Funcionalidades implementadas

- Descoberta automática, atualizada a cada quatro segundos, de serviços de pareamento e conexão.
- Modelo, versão de API e versão ADB Wi-Fi quando fornecidos pelo ADB moderno. A descoberta antiga continua disponível como alternativa e não inventa metadados ausentes.
- Busca por modelo, nome de instância e endereço.
- Pareamento com código de seis dígitos ou QR code compatível com o protocolo do Android Studio.
- Sessões QR com identidade e segredo criptograficamente aleatórios, renovação, encerramento e expiração em três minutos.
- Conexão e desconexão manuais por IP e porta; listagem do estado real dos dispositivos no ADB.
- Localização automática do Android SDK, com caminho de `adb.exe` configurável na janela.
- Processos sem shell intermediário, timeout e cancelamento. O código de pareamento é transmitido pela entrada padrão e removido das mensagens retornadas.

## Requisitos e uso

Use Windows, Visual Studio com suporte a VisualStudio.Extensibility (o pacote declara VS 2022 17.14 ou superior), Android SDK Platform-Tools e um aparelho com Android 11 ou superior. O carregamento da janela no Visual Studio foi confirmado por captura fornecida pelo usuário. A correção da exibição do QR no primeiro clique ainda precisa ser confirmada no host; veja os detalhes em docs/validacao.md.

Após carregar a extensão no IDE, abra **Ferramentas → Android via Wi-Fi**.

1. Conecte computador e aparelho à mesma rede. Ative **Opções do desenvolvedor → Depuração sem fio** no Android.
2. Para QR code, clique em **Gerar novo QR code** e use **Parear dispositivo com QR code** no Android. A extensão aceita somente o anúncio cujo nome corresponde exatamente à sessão criada.
3. Para código, abra **Parear dispositivo com código de pareamento**, selecione o serviço correspondente ou informe seu endereço e o código temporário.
4. Confira a lista **Dispositivos reconhecidos pelo ADB**. O estado `device` indica comunicação online. Pareamento bem-sucedido, isoladamente, não comprova conexão.
5. Se o ADB não conectar automaticamente, selecione o serviço de conexão ou copie o endereço da tela principal de Depuração sem fio para a aba **Conexão manual**.

As portas de pareamento e conexão são diferentes e podem mudar. A extensão não associa aparelhos apenas por IP nem reutiliza a porta de pareamento para conectar. Redes que bloqueiam multicast mDNS podem exigir entrada manual. Para aparecer nos destinos de depuração do Visual Studio, o aparelho deve estar online no servidor ADB usado pelo SDK do IDE; caminhos para SDKs diferentes podem exigir ajuste em **Configuração do Android SDK e diagnóstico**.

## Compilar e testar

Na raiz do projeto:

```powershell
dotnet build DiscoveryDeviceAndroid.slnx -c Release
dotnet test DiscoveryDeviceAndroid.Tests -c Release --no-build
dotnet test DiscoveryDeviceAndroid.UiTests -c Release --no-build
```

Também é possível executar `scripts/Verificar.ps1`. O VSIX é gerado em `DiscoveryDeviceAndroid/bin/Release/net8.0-windows8.0/DiscoveryDeviceAndroid.vsix`.

O projeto original usava `Microsoft.VisualStudio.Sdk.Build` e o destino `vs18_11`, que não resolviam neste ambiente. A implementação usa o formato publicado nos exemplos oficiais da Microsoft: `Microsoft.NET.Sdk`, `net8.0-windows8.0` e pacotes VisualStudio.Extensibility 17.14.40608. Não foi necessário instalar outro SDK ou alterar a instalação do Visual Studio.

## Distribuir e publicar

Execute `.\scripts\Empacotar.ps1` para gerar o VSIX versionado, um ZIP para compartilhar com os desenvolvedores e um ZIP com os materiais do Marketplace. A saída padrão fica em `dist/AndroidWifi-1.0.1-preview`. O script preserva pacotes anteriores; se o destino já existir, informe outro diretório com `-DiretorioSaida`.

As instruções estão em [Instalação](docs/INSTALACAO.md) e [Publicação](docs/PUBLICACAO.md). A geração local não publica a extensão nem cria uma conta de publisher. Confirme seu publisher antes da publicação pública. A extensão é distribuída sob a [licença MIT](LICENSE.txt); as dependências mantêm suas respectivas licenças e avisos.

## Organização dos arquivos

- `DiscoveryDeviceAndroid/Pareamento`: comando, janela, apresentação e sessão QR. XAML e C# separados.
- `DiscoveryDeviceAndroid.Core`: descoberta, dispositivos, conexão e execução de processos. Sem dependência do Visual Studio.
- `DiscoveryDeviceAndroid.Tests`: testes do protocolo textual, endereços, segurança do segredo e processos.
- `DiscoveryDeviceAndroid.UiTests`: renderização WPF real do QR e leitura por uma biblioteca independente, além de renovação e expiração da sessão.
- `docs/android-research.md`: engenharia reversa do Android Studio instalado, fontes primárias, evidências e limites.
- `docs/validacao.md`: resultados e pendências da validação local.

## Limites

O projeto recria a funcionalidade e usa o ADB instalado. Não reproduz pixel a pixel os diálogos do Android Studio. A compatibilidade de protocolo foi investigada por bytecode e documentação; o pareamento físico e uma sessão de depuração do Visual Studio ainda precisam ser validados no aparelho.

Referências: [exemplo oficial de ToolWindow](https://github.com/microsoft/VSExtensibility/tree/main/New_Extensibility_Model/Samples/ToolWindowSample), [Remote UI](https://learn.microsoft.com/visualstudio/extensibility/visualstudio.extensibility/inside-the-sdk/remote-ui), [protocolo ADB Wi-Fi](https://android.googlesource.com/platform/packages/modules/adb/+/HEAD/docs/dev/adb_wifi.md).
