# Como contribuir

Correções, documentação e relatos reproduzíveis são bem-vindos. Para mudanças grandes, abra uma proposta antes de iniciar a implementação. Consulte o [código de conduta](CODE_OF_CONDUCT.md) e a [política de segurança](SECURITY.md).

## Ambiente

- Windows x64 e PowerShell 7.2 ou superior.
- SDK .NET 10 estável, selecionado por `global.json`.
- Runtime .NET 8 Desktop para executar os testes WPF. Instalar o SDK .NET 8 também o fornece.
- Visual Studio compatível e Android SDK somente para testar a integração real. Os testes automatizados não exigem aparelho nem ADB instalado.

```powershell
git clone https://github.com/afernandes/DiscoveryDeviceAndroid.git
cd DiscoveryDeviceAndroid
git switch -c fix/descricao-do-problema
pwsh ./scripts/Verificar.ps1
```

## Antes do PR

Execute `pwsh ./scripts/Verificar.ps1`: valida os metadados e links locais, restaura em modo bloqueado, confere a formatação, compila e roda as duas suítes. Para gerar os pacotes, execute `pwsh ./scripts/Empacotar.ps1 -DiretorioSaida ./artifacts/minha-validacao`.

Para corrigir somente espaços e indentação:

```powershell
dotnet format whitespace DiscoveryDeviceAndroid.slnx --no-restore
```

Ao alterar dependências, atualize intencionalmente os arquivos `packages.lock.json` com `dotnet restore DiscoveryDeviceAndroid.slnx --force-evaluate`. Não desabilite a auditoria NuGet para fazer o CI passar.

## Convenções

- Uma classe por arquivo; namespaces existentes e organização por responsabilidade/feature.
- Identificadores de domínio em português; padrões de infraestrutura em inglês.
- Testes no padrão `Metodo_Cenario_ResultadoEsperado`.
- Markup XAML separado de C#; sem trabalho bloqueante de rede/processo na interface.
- Não passar códigos de pareamento em argumentos de processo, logs, issues ou screenshots.
- Preservar a distinção entre pareamento e conexão e a comparação exata da instância QR.
- Datas em UTC com `DateTimeOffset`; cancelamento e timeout para I/O.

## Revisão e integração

A `main` é atualizada por PR. O CI obrigatório precisa passar e as conversas precisam estar resolvidas. O repositório usa squash merge e exclui a branch de trabalho após a integração. `CODEOWNERS` indica o responsável; aprovação externa não é obrigatória enquanto houver apenas um mantenedor. Veja a [política de manutenção](docs/MANUTENCAO.md).

Explique o problema, o resultado e a validação. Diferencie leitura de código, testes executados, teste no IDE e teste físico. Não declare homologação com base somente no build.

As contribuições são distribuídas sob a [MIT](LICENSE.txt). Não envie binários proprietários do Android Studio, chaves ADB, arquivos de usuário do IDE, `bin`, `obj`, `.vs` ou pacotes `dist`.
