# Revisão do repositório — 26/10/2026

## Evidência inicial

| Achado | Evidência antes da alteração | Decisão |
|---|---|---|
| Repositório público sem proteção | API `repos/afernandes/DiscoveryDeviceAndroid/rulesets`: lista vazia; `branches/main/protection`: 404 | PR, checks, conversas resolvidas, histórico linear e bloqueio de força/exclusão. |
| Sem workflows | Árvore inicial não continha `.github/workflows` | CI Windows, CodeQL e release manual. |
| Três estratégias de merge e branches persistentes | API do repositório: merge/rebase/squash habilitados e delete_branch_on_merge falso | Somente squash e exclusão da branch de trabalho. |
| Falta de guias e templates | Community profile sem contributing, código de conduta e templates | Documentação comunitária, formulários de issue e PR. |
| SDK de compilação não fixado | `global.json` ausente; build anterior usava SDK 11 RC | .NET 10 estável e runtime 8 para testes WPF. |
| Restauração dependia de fontes locais | Ausência de `NuGet.config` e lock files | nuget.org explícito e restauração bloqueada. |
| Vulnerabilidades transitivas de MessagePack | Restore com `NuGetAuditMode=all` produziu NU1902/NU1903 para 2.5.198 | Referência 2.5.307 na mesma linha, lock files e restauração sem esses avisos. |
| Conteúdo gerado muito grande no Git | `THIRD-PARTY-NOTICES.txt` contribuía com cerca de 80 mil linhas | Removido do índice, preservado localmente, ignorado e gerado no empacotamento. |
| Testes continham identificadores capturados | Fixture de `MdnsSnapshotTests` e registros de investigação | Exemplos públicos com identificadores e endereços de documentação. Sem reescrever o histórico. |
| Release parcialmente dependia de comandos ad hoc | Publicação inicial via CLI e hashes conferidos manualmente | Script versionado que verifica upload antes de publicar e recusa sobrescrita. |

## Limites

A evidência acima descreve o estado inicial e as decisões. Os resultados finais do CI, CodeQL, PR e configuração devem ser conferidos nos próprios checks e APIs após a integração. Os testes locais não comprovam o pareamento físico ou a aceitação no Marketplace.

O advisory de MessagePack descreve a dependência e as condições de impacto, não uma exploração reproduzida nesta extensão. O SDK do Visual Studio pode fornecer componentes no host; corrigir o grafo do projeto não atualiza o IDE instalado. Referências: [GHSA-hv8m-jj95-wg3x](https://github.com/advisories/GHSA-hv8m-jj95-wg3x), [GHSA-vh6j-jc39-fggf](https://github.com/advisories/GHSA-vh6j-jc39-fggf).

## Recomendações que dependem de validação posterior

Homologação em aparelhos e versões de IDE, screenshots sem dados de outros projetos, assinatura de código, publicação no Marketplace e eventual revisão de responsabilidades do ViewModel. Não foram adicionadas pastas de camadas, frameworks de teste ou serviços externos apenas para uniformizar a aparência do projeto.
