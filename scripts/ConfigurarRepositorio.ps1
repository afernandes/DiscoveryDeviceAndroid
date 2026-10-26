#requires -Version 7.2
param([switch]$AplicarProtecao)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$config = Get-Content (Join-Path $raiz '.github/repository-settings.json') -Raw | ConvertFrom-Json
$repo = 'afernandes/DiscoveryDeviceAndroid'

function Invoke-GitHubJson {
    param([string]$Endpoint, [string]$Method = 'GET', [object]$Body)
    $arquivo = $null
    try {
        $opcoes = @('api', $Endpoint, '--method', $Method)
        if ($null -ne $Body) {
            $arquivo = Join-Path ([IO.Path]::GetTempPath()) ('android-wifi-github-' + [guid]::NewGuid().ToString('N') + '.json')
            $Body | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $arquivo -Encoding utf8
            $opcoes += @('--input', $arquivo)
        }
        $saida = & gh @opcoes
        if ($LASTEXITCODE -ne 0) { throw "Falha em $Method $Endpoint" }
        if ($saida) { return ($saida -join "`n" | ConvertFrom-Json) }
    }
    finally { if ($arquivo -and (Test-Path -LiteralPath $arquivo)) { Remove-Item -LiteralPath $arquivo } }
}

Invoke-GitHubJson -Endpoint "repos/$repo" -Method PATCH -Body $config.repository | Out-Null
Invoke-GitHubJson -Endpoint "repos/$repo/topics" -Method PUT -Body @{ names = @($config.topics) } | Out-Null
Invoke-GitHubJson -Endpoint "repos/$repo/vulnerability-alerts" -Method PUT | Out-Null
Invoke-GitHubJson -Endpoint "repos/$repo/automated-security-fixes" -Method PUT | Out-Null
Invoke-GitHubJson -Endpoint "repos/$repo/private-vulnerability-reporting" -Method PUT | Out-Null
Invoke-GitHubJson -Endpoint "repos/$repo/actions/permissions" -Method PUT -Body $config.actions | Out-Null
Invoke-GitHubJson -Endpoint "repos/$repo/actions/permissions/selected-actions" -Method PUT -Body $config.selectedActions | Out-Null
foreach ($label in $config.labels) {
    & gh label create $label.name --repo $repo --color $label.color --description $label.description --force
    if ($LASTEXITCODE -ne 0) { throw "Falha ao configurar label $($label.name)." }
}
if ($AplicarProtecao) {
    $main = Invoke-GitHubJson -Endpoint "repos/$repo/commits/main"
    $checks = Invoke-GitHubJson -Endpoint "repos/$repo/commits/$($main.sha)/check-runs?per_page=100"
    foreach ($nome in @('Validar Windows', 'Analisar CSharp')) {
        if (@($checks.check_runs | Where-Object { $_.name -eq $nome -and $_.conclusion -eq 'success' }).Count -eq 0) {
            throw "O check obrigatório '$nome' ainda não passou em main. As proteções não foram alteradas."
        }
    }
    $rulesets = @(Invoke-GitHubJson -Endpoint "repos/$repo/rulesets")
    foreach ($desejado in @($config.mainRuleset, $config.tagRuleset)) {
        $existente = @($rulesets | Where-Object name -eq $desejado.name)
        if ($existente.Count -gt 1) { throw 'Há rulesets duplicados com o mesmo nome.' }
        if ($existente.Count -eq 1) {
            Invoke-GitHubJson -Endpoint "repos/$repo/rulesets/$($existente[0].id)" -Method PUT -Body $desejado | Out-Null
        }
        else { Invoke-GitHubJson -Endpoint "repos/$repo/rulesets" -Method POST -Body $desejado | Out-Null }
    }
    Invoke-GitHubJson -Endpoint "repos/$repo/environments/release" -Method PUT -Body @{
        deployment_branch_policy = @{ protected_branches = $true; custom_branch_policies = $false }
    } | Out-Null
}
Write-Output 'Configurações solicitadas aplicadas ao GitHub. Consulte os endpoints para verificar o estado atual.'
