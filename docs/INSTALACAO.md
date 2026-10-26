# Instalar Android via Wi-Fi

## Instalação pelo arquivo VSIX

1. Extraia o ZIP recebido para uma pasta local.
2. Salve seu trabalho e feche o Visual Studio, caso o instalador solicite.
3. Abra `AndroidWifi-1.0.1-preview.vsix` com dois cliques.
4. Selecione a instalação compatível do Visual Studio e conclua o instalador.
5. Abra o IDE e acesse **Ferramentas → Android via Wi-Fi**.

Distribua o VSIX completo. Os outros desenvolvedores não precisam do código-fonte nem de uma instalação do Android Studio. Precisam de Visual Studio compatível, Android SDK Platform-Tools e Android 11 ou superior no aparelho.

Compatibilidade declarada deste pacote Preview: Windows x64, Visual Studio 2022 17.14 e Visual Studio 2026. Outras arquiteturas não são declaradas neste pacote. Nem todas as combinações de IDE e aparelho foram validadas.

## Primeiro uso

Ative **Opções do desenvolvedor → Depuração sem fio** no Android. Computador e aparelho precisam comunicar-se pela rede Wi-Fi. Escolha QR code ou código de pareamento na janela da extensão.

O ADB é localizado automaticamente. Se necessário, informe seu caminho em **Configuração do Android SDK e diagnóstico**, usando o SDK configurado no Visual Studio.

Pareamento e conexão usam portas diferentes. Para conexão manual, copie o IP e a porta da tela principal de Depuração sem fio. Para pareamento, use os dados da tela de código de pareamento.

## Atualização e remoção

Instale o novo VSIX quando houver uma versão maior. O ID da extensão é mantido para que a atualização substitua a mesma extensão. Para remover, use o gerenciador de extensões do Visual Studio.

## Se o instalador falhar

Confira a versão e arquitetura do IDE e as políticas de instalação da organização. Na máquina de desenvolvimento foi observado um erro do instalador moderno, `Multiple targets are currently not supported for this operation`. O carregamento da janela foi depois confirmado por captura, mas a causa desse erro não foi demonstrada. Se ocorrer na máquina de destino, registre a versão do IDE e o log do VSIXInstaller para diagnóstico.

Este VSIX não recebeu assinatura de certificado do autor. Não há necessidade de desativar proteções do Windows ou o firewall para testá-lo.

## Limites da prévia

Build e testes automatizados foram executados. O fluxo físico completo de pareamento e uma sessão real de depuração ainda precisam ser confirmados. A correção da exibição inicial do QR foi verificada com WPF e um leitor independente; falta a confirmação do primeiro clique dentro do host remoto.
