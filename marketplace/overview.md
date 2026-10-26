# Android via Wi-Fi

Descubra e conecte dispositivos Android ao ADB pela rede sem fio, diretamente em uma janela do Visual Studio.

## Recursos

- Descoberta automática por mDNS, com busca por dispositivo ou endereço.
- Modelo, API e versão ADB Wi-Fi quando esses dados são anunciados.
- Pareamento com código de seis dígitos ou QR code temporário.
- Sessões QR com renovação e expiração em três minutos.
- Conexão e desconexão por IP e porta, para redes sem descoberta automática.
- Lista de dispositivos e estados informados pelo ADB.
- Interface em português do Brasil.

## Como usar

1. Abra **Ferramentas → Android via Wi-Fi**.
2. Conecte o computador e o aparelho à mesma rede.
3. No Android, ative **Opções do desenvolvedor → Depuração sem fio**.
4. Escolha pareamento por QR code ou código de seis dígitos.
5. Confira o estado `device` na lista do ADB. Se não conectar automaticamente, use o IP e a porta da tela principal de Depuração sem fio.

As portas de pareamento e de conexão são diferentes. Redes que bloqueiam multicast mDNS podem exigir a conexão manual.

## Requisitos

- Windows x64.
- Compatibilidade declarada com Visual Studio 2022 17.14 e Visual Studio 2026.
- Android SDK Platform-Tools instalado e dispositivo com Android 11 ou superior.
- Computador e dispositivo com comunicação permitida na mesma rede.

A extensão procura o ADB instalado. Também é possível informar o caminho do `adb.exe` usado pelo SDK do seu Visual Studio.

## Prévia

Esta é uma versão Preview. O carregamento da janela e a descoberta de um SM-X820 foram observados. Há testes automatizados de protocolo, processos e renderização/decodificação do QR. O pareamento físico completo, a correção do primeiro clique no host e uma sessão de depuração real ainda precisam de confirmação. A compatibilidade declarada não significa homologação de todas as versões do IDE.

## Licença

Distribuída sob a licença MIT, incluída no pacote. As dependências mantêm suas respectivas licenças e avisos de terceiros.

## Dados e privacidade

A extensão não implementa telemetria própria nem envia dados para serviços de análise. Ela executa o ADB local para descoberta, pareamento e conexão com o aparelho. O ADB mantém suas próprias credenciais e configurações. O código de pareamento é passado pela entrada padrão do processo e não faz parte de sua linha de comando.

Não há vínculo oficial com Google, Android Studio ou Microsoft. Android e Visual Studio são marcas de seus respectivos titulares.
