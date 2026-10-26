# Segurança

## Versões e correções

O projeto está em Preview. Correções são concentradas na versão mais recente do código e serão distribuídas em uma nova release; binários anteriores não são sobrescritos. Não há compromisso de backport ou prazo de atendimento.

A auditoria NuGet inclui dependências transitivas e bloqueia a restauração quando uma vulnerabilidade produz aviso. Dependabot monitora NuGet e Actions; CodeQL analisa C#. Um build aprovado não comprova ausência de vulnerabilidades nem segurança do próprio Android SDK ou do Visual Studio.

## Relato privado

Use [Report a vulnerability](https://github.com/afernandes/DiscoveryDeviceAndroid/security/advisories/new). Esse canal é privado e evita expor detalhes antes da análise. Não abra uma issue pública com chaves, códigos temporários, capturas de rede identificáveis ou instruções de exploração.

Inclua versão da extensão/IDE/ADB, passos mínimos de reprodução, impacto esperado e uma evidência com dados fictícios. O mantenedor analisará o relato e coordenará a divulgação e eventual advisory quando aplicável.

## Limites de confiança

A extensão delega o protocolo criptográfico ao ADB instalado. Dados de descoberta são tratados como entrada externa, e um anúncio QR somente é aceito se sua instância corresponder à sessão criada. As operações físicas exigem ação do usuário. Não existe servidor de telemetria próprio.

O código de pareamento é enviado por stdin; mensagens do cliente removem o segredo. Anúncios de rede, respostas de subprocessos e logs não devem ser considerados instruções confiáveis.
