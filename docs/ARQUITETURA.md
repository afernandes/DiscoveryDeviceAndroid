# Arquitetura

A solução mantém o protocolo e o processo ADB fora da dependência de Visual Studio. Não reimplementa o transporte criptográfico do ADB nem incorpora binários do Android Studio.

| Projeto | Responsabilidade |
|---|---|
| `DiscoveryDeviceAndroid` | Comando, ToolWindow, Remote UI e apresentação do pareamento. |
| `DiscoveryDeviceAndroid.Core` | Descoberta, parsing, validação de endereço e execução controlada do ADB. |
| `DiscoveryDeviceAndroid.Tests` | Contratos de protocolo, subprocessos, timeout e cancelamento. |
| `DiscoveryDeviceAndroid.UiTests` | Trecho XAML real e decodificação independente do QR renderizado. |

As pastas existentes seguem responsabilidades. Não é necessário mover os projetos para `src` e `tests` apenas por convenção: a separação já é explícita e os nomes fazem parte de scripts e referências.

## Descoberta e conexão

`ClienteAdb` prefere um snapshot finito de `adb mdns track-services --proto-text` e recorre a `adb mdns services` quando a forma moderna não estiver disponível. O snapshot possui limites de tempo e tamanho. Apenas o processo cliente criado pela extensão é encerrado; o servidor ADB é compartilhado com o IDE.

`_adb-tls-pairing._tcp` anuncia o endpoint de pareamento; `_adb-tls-connect._tcp` anuncia o de conexão. Sucesso em `pair` não equivale a `device` online. Metadados não anunciados não são inventados. Endereços manuais precisam passar pelo contrato de validação de IP e porta.

## QR e apresentação

A sessão gera uma instância aleatória e um segredo temporário no formato `WIFI:T:ADB;S:<instância>;P:<segredo>;;`. A descoberta aceita apenas a instância exata daquela sessão. O ViewModel envia tamanho e geometria para um objeto estável; `Viewbox`, `Canvas` e `Path` desenham o QR no XAML do host.

O teste WPF decodifica os pixels com uma segunda biblioteca. Isso verifica a geometria e o binding local; a câmera do Android e o transporte Remote UI dependem de validação de integração.

## Dependências e distribuição

O SDK público VisualStudio.Extensibility é referenciado pelo projeto da extensão. A referência explícita de MessagePack permanece na linha 2.5 para reduzir a mudança de contrato e corrigir os advisories do grafo local. Não é uma atualização da instalação do Visual Studio nem prova de qual biblioteca o host do IDE utiliza em runtime.

O ADB continua sendo obtido do SDK do usuário. Os avisos das bibliotecas efetivamente selecionadas para o VSIX são gerados durante o empacotamento. Lock files e hashes controlam versões e integridade; não tornam o ZIP byte a byte reproduzível, pois ele contém timestamps e metadados da execução.
