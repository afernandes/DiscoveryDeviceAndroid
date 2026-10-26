# Validação local inicial — 26/10/2026

> Registro histórico. O carregamento da janela foi confirmado posteriormente pela captura do usuário, conforme a atualização ao final. Os testes atuais são executados pelo CI. Os identificadores e endereços dos exemplos foram anonimizados; números de versão e conclusões foram preservados.

## Resultado reproduzível

`scripts/Verificar.ps1` concluiu com sucesso:

- Build Release da solução: zero erros e zero avisos.
- DiscoveryDeviceAndroid.Tests: 32 testes aprovados, nenhum ignorado.
- DiscoveryDeviceAndroid.UiTests: dois testes aprovados, nenhum ignorado.
- VSIX gerado: `DiscoveryDeviceAndroid/bin/Release/net8.0-windows8.0/DiscoveryDeviceAndroid.vsix`.

SDK usado: .NET 11.0.100-rc.3.26522.148, com destino de execução .NET 8. O SDK emite a mensagem informativa NETSDK1057 por ser uma versão de pré-lançamento; não foi contabilizada como aviso de compilação. Há runtimes .NET 8 instalados nesta máquina.

Os testes de QR carregam o trecho XAML real da extensão, renderizam o Canvas/Path com WPF e usam ZXing.Net para decodificar os pixels. Eles verificam a primeira geração após o binding inicialmente vazio, a renovação e uma nova geração após limpar a sessão. O texto recuperado precisa coincidir exatamente com o payload ADB e com o nome e segredo da sessão. Isso comprova a geração e legibilidade do QR no renderer testado; não comprova leitura pela câmera física nem o transporte Remote UI dentro do Visual Studio.

## Verificação real somente de leitura

O ADB instalado, versão `37.0.1-15733141`, foi localizado em `C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe`. A coleta real de `mdns track-services --proto-text` e sua interpretação pelo Core retornaram:

| Campo | Valor observado |
|---|---|
| Modelo | SM-X820 |
| API | 36.1 |
| Versão mDNS | 1 |
| Tipo | `_adb-tls-connect._tcp` |
| Endereço naquele instante | 192.0.2.10:39775 |
| Dispositivo conhecido | true |

O aparelho já estava autorizado e online no ADB. Isso não é resultado de um novo pareamento feito pela extensão. Não foram executados `pair`, `connect`, `disconnect` nem reinicialização do servidor ADB contra esse aparelho. Os testes dessas operações usam executor controlado.

## Histórico da investigação do host Visual Studio

O Visual Studio Professional 2026 instalado é 18.11.12324.101. O projeto inicial não compilava porque `Microsoft.VisualStudio.Sdk.Build` não era resolvido nem pelo `dotnet build` nem pelo MSBuild da instalação. O formato foi substituído pelo SDK .NET e pelos pacotes publicados usados nos exemplos oficiais da Microsoft.

Na tentativa inicial, o empacotamento funcionava, mas o carregamento no IDE ainda não havia sido confirmado:

1. `VSIXInstaller /quiet /rootSuffix:Exp /instanceIds:4eaf4948` retornou `RequiresInstallerException`, com a mensagem `Multiple targets are currently not supported for this operation`.
2. A seleção explícita de `appIdInstallPath`, `appIdName`, SKU e versão retornou a mesma mensagem.
3. `Build;DeployVsixExtensionFiles`, com MSBuild da instalação e sufixo Exp, copiou o pacote para o diretório experimental. O comando não apareceu no menu Ferramentas observado.
4. A inspeção estática do ExtensionEngine instalado revelou o mecanismo `HotloadRegistration.txt`. Foi adicionada, somente no hive experimental, a linha do diretório desta extensão. A inspeção subsequente ainda não confirmou seu registro no IDE.

O caminho do arquivo de registro foi confirmado por uma chamada somente de leitura à API `ExternalSettingsManager.CreateForApplication(devenvPath, "Exp", false).GetApplicationDataFolder(ApplicationDataFolder.UserExtensions)`. Ela retornou exatamente o diretório `18.0_4eaf4948Exp\Extensions` acima. Portanto, a posição do arquivo foi verificada; o motivo de a extensão não aparecer continua não resolvido.

Diretório de desenvolvimento preservado:

```text
%LOCALAPPDATA%\Microsoft\VisualStudio\18.0_4eaf4948Exp\Extensions\afernandes\Android via Wi-Fi\1.0.0.0
```

Arquivo de registro de desenvolvimento adicionado:

```text
%LOCALAPPDATA%\Microsoft\VisualStudio\18.0_4eaf4948Exp\Extensions\HotloadRegistration.txt
```

Nenhuma instalação bem-sucedida no hive principal foi confirmada. A causa do erro do instalador não foi demonstrada; não atribuí-la às duas arquiteturas do manifesto, à versão do runtime ou a uma mudança do usuário sem evidência adicional.

## Critérios ainda não verificados

- Carregamento do comando e da janela Remote UI no Visual Studio.
- Renderização completa da janela com temas, escala e teclado no host real.
- Pareamento de aparelho físico por código e QR.
- Conexão feita pela extensão e aparecimento do aparelho nos destinos de depuração do IDE.
- Instalação em outra versão/edição do Visual Studio ou arquitetura ARM64.

A recriação do código e do pacote está entregue, mas a funcionalidade completa dentro do Visual Studio ainda depende de resolver e validar o carregamento no host e o fluxo físico.

## Atualização: exibição inicial do QR

Uma captura posterior fornecida pelo usuário confirma o carregamento da janela da extensão no Visual Studio, a descoberta do SM-X820 e a sessão QR ativa. O usuário relatou que a imagem apareceu apenas ao clicar pela segunda vez. Assim, o bloqueio de carregamento descrito acima corresponde à tentativa inicial; o carregamento da janela está confirmado pela captura, sem atribuir uma causa à recuperação.

A apresentação do QR foi alterada: em vez de enviar um `XamlFragment` contendo um `DrawingImage` novo a cada geração, a extensão mantém um objeto de apresentação estável e transmite somente tamanho inteiro e geometria textual. O próprio XAML desenha o QR com Viewbox, Canvas e Path. O formato de pareamento e as credenciais de sessão permanecem os mesmos.

O teste de UI usa o trecho XAML da extensão com binding vazio e valida, por decodificação independente dos pixels, a primeira geração, a renovação e a geração após limpar. Isso verifica o novo caminho WPF; não reproduz o transporte remoto do IDE nem demonstra a causa exata do atraso anterior. A confirmação da correção do primeiro clique no host real ainda está pendente.