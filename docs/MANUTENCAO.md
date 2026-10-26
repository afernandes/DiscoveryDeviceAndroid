# Manutenção e políticas

## Pull requests

`main` recebe alterações por PR e merge por squash. A política exige os checks **Validar Windows** e **Analisar CSharp**, atualização em relação à base e resolução das conversas. Force push e exclusão da branch são bloqueados, sem exceção permanente de administrador. O administrador continua podendo editar a política para recuperação explícita.

`CODEOWNERS` encaminha a responsabilidade para `@afernandes`. O número de aprovações obrigatórias é zero enquanto há um único mantenedor: isso mantém PR e CI obrigatórios sem exigir uma aprovação que o próprio autor não pode fornecer. Ao adicionar outro mantenedor, recomenda-se exigir uma aprovação externa e aprovação de code owner.

Branches de trabalho são excluídas após a integração. Tags `v*` não podem ser atualizadas ou excluídas pela política de versões; novas tags continuam permitidas. Não são exigidos commits assinados, merge queue ou bots adicionais sem uma necessidade demonstrada.

## CI

O workflow [ci.yml](../.github/workflows/ci.yml) usa Windows e instala SDKs .NET 8/10. `global.json` seleciona um SDK 10 estável; o runtime 8 atende os testes WPF. Os pacotes são restaurados com `--locked-mode` e cache baseado em `packages.lock.json`.

O job valida JSON/XML, sintaxe dos scripts e links locais, confere formatação, compila, executa testes e verifica o conteúdo do VSIX. Relatórios TRX e distribuição são armazenados como artefatos. A execução não conecta aparelhos nem efetua homologação do IDE.

Os workflows usam Actions oficiais fixadas por SHA, tokens de leitura por padrão e nenhum `pull_request_target`. Dependabot abre PRs semanais de NuGet e Actions; não faz merge automático. CodeQL analisa C# nos PRs, em `main` e semanalmente. A auditoria NuGet inclui transitivas e não ignora advisories para liberar o build.

## Release no GitHub

1. Atualize `Version` em `Directory.Build.props`, o `CHANGELOG.md` e `Distribution/RELEASE-NOTES.txt` pelo PR.
2. Aguarde CI e CodeQL e integre em `main`.
3. Em **Actions → Release → Run workflow**, escolha `main` e informe `v` seguido da versão, por exemplo `v1.0.2`.
4. O workflow recompila e testa o commit selecionado, cria o pacote e entrega esse artefato ao job de publicação, sem recompilá-lo nesse job.
5. O job publica primeiro um rascunho, verifica tamanho e SHA-256 de cada upload e somente então o disponibiliza. A indicação Preview vem dos metadados do VSIX.

O workflow rejeita versões divergentes e tags existentes. Não executa ao abrir PR nem ao enviar qualquer tag. Não sobrescreve a release 1.0.1 já publicada e não altera a visibilidade do repositório. A publicação usa `GITHUB_TOKEN`, sem PAT permanente. O environment `release` permite somente branches protegidas.

Publicação no Visual Studio Marketplace permanece manual, conforme [PUBLICACAO.md](PUBLICACAO.md), até haver publisher e autenticação definidos. Assinatura de código com certificado e homologação física são etapas futuras, sem certificações implícitas.

## Configurações do GitHub como código

Os valores desejados ficam em [.github/repository-settings.json](../.github/repository-settings.json). Um administrador autenticado pode aplicar:

```powershell
pwsh ./scripts/ConfigurarRepositorio.ps1
pwsh ./scripts/ConfigurarRepositorio.ps1 -AplicarProtecao
```

O segundo comando aplica rulesets e o environment de publicação somente quando os checks obrigatórios já existem com sucesso no commit atual de `main`. Isso evita exigir um check que nunca foi registrado. O script não muda a visibilidade nem remove releases. Mudanças de configuração passam a ser registradas em PR antes de serem reaplicadas.

## Itens a reavaliar

- Exigir aprovação externa quando houver mais mantenedores.
- Adicionar novas versões/arquiteturas à matriz apenas após validar em máquinas reais.
- Publicar uma nova versão com o grafo corrigido; a release anterior continua sendo um registro imutável.
- Revisar alertas do CodeQL e Dependabot periodicamente; configuração habilitada não significa ausência de alertas.
