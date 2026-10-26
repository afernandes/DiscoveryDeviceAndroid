# Validação de integração

Os testes automatizados não acessam aparelhos. Use esta matriz para registrar evidências reais de uma versão antes de ampliar as declarações de suporte. Marque um cenário como aprovado somente quando executado.

| Cenário | Evidência esperada | Estado inicial |
|---|---|---|
| Instalação limpa do VSIX | IDE/edição/versão, instalação concluída e comando disponível | Não verificado |
| Primeiro QR e renovação | QR visível no primeiro clique e após encerrar/gerar novamente | WPF testado; host pendente |
| Pareamento por QR | Leitura pela câmera e confirmação do ADB | Não verificado |
| Pareamento por código | Código válido, inválido e expirado, com mensagens claras | Não verificado em aparelho |
| Conexão no IDE | Estado `device` e aparelho no seletor de destino | Não verificado |
| Depuração de aplicação | Implantação, breakpoint e comunicação sem USB | Não verificado |
| Rede sem mDNS | Diagnóstico e uso de endereço manual | Não verificado |
| Desconexão/queda de Wi-Fi | Estado atualizado sem bloquear o IDE | Não verificado |
| Fechar/reabrir a janela | Cancelamento das operações e nova sessão funcional | Não verificado |
| Tema, DPI e teclado | Escuro/claro, escala de 125/150/200%, navegação por Tab | Não verificado |

Registre versão da extensão, commit, IDE, Windows, Android, Platform-Tools e data. Use identificadores fictícios em material público. O histórico local de descoberta de um SM-X820 está em [validacao.md](validacao.md); ele não comprova os cenários acima.
