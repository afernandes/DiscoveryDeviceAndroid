# Histórico de alterações

As versões publicadas estão em [GitHub Releases](https://github.com/afernandes/DiscoveryDeviceAndroid/releases). Alterações ainda não publicadas permanecem nesta seção até uma nova release.

## Não publicado

- README focado em instalação e uso; documentação de arquitetura, validação e manutenção.
- Templates de issues/PR, guias de contribuição, segurança e código de conduta.
- CI Windows com SDK estável, lock files, auditoria transitiva e artefatos de distribuição.
- CodeQL, Dependabot e publicação de releases por acionamento manual em `main`.
- Proteção de PRs, squash merge e política de branches.
- Referência explícita a MessagePack 2.5.307, corrigindo os advisories identificados na dependência 2.5.198 do SDK. A análise não demonstrou exploração no produto; o runtime de comunicação provido pelo Visual Studio precisa de validação própria.
- Avisos de terceiros gerados durante o empacotamento, sem versionar o arquivo derivado.

## 1.0.1 Preview — 2026-10-26

- Descoberta automática por mDNS e metadados do dispositivo quando disponíveis.
- Pareamento por código e QR temporário; conexão/desconexão por endereço.
- Exibição vetorial do QR com teste de geração inicial, renovação e limpeza.
- Pacotes de instalação e materiais do Marketplace, licença MIT e hashes SHA-256.

## Política de versões

O ID do VSIX e o nome interno do Marketplace permanecem estáveis. A versão é declarada em `Directory.Build.props`. Antes de publicar, o mantenedor incrementa a versão, atualiza as notas e o changelog e integra o PR. O workflow rejeita uma tag que não corresponda à versão do pacote e não sobrescreve tags existentes.
