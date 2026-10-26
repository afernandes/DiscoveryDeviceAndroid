#requires -Version 7.2
# Testa as barreiras de publicação com gh simulado: nenhuma chamada de rede é executada.
$ErrorActionPreference = 'Stop'
$publicador = Join-Path $PSScriptRoot 'PublicarRelease.ps1'
$pasta = Join-Path ([IO.Path]::GetTempPath()) ('android-wifi-publish-test-' + [guid]::NewGuid().ToString('N'))
$repoAnterior = $env:GH_REPO
$commit = '1111111111111111111111111111111111111111'
$global:AndroidWifiReleaseTestState = [pscustomobject]@{
    Mode = 'sucesso'
    Calls = [Collections.Generic.List[string]]::new()
    Pasta = $pasta
}

function gh {
    $global:AndroidWifiReleaseTestState.Calls.Add(($args -join ' '))
    $global:LASTEXITCODE = 0
    if ($args[0] -eq 'api' -and $args[1] -like '*matching-refs*') {
        if ($global:AndroidWifiReleaseTestState.Mode -eq 'tag-existente') { return '[{"ref":"refs/tags/v9.9.9"}]' }
        return '[]'
    }
    if ($args[0] -eq 'api' -and $args[1] -like '*/releases') {
        $assets = foreach ($nome in @('teste.vsix', 'teste-devs.zip', 'teste-marketplace.zip', 'SHA256SUMS.txt')) {
            $arquivo = Get-Item -LiteralPath (Join-Path $pasta $nome)
            $digest = 'sha256:' + (Get-FileHash $arquivo.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            if ($global:AndroidWifiReleaseTestState.Mode -eq 'upload-divergente' -and $nome -eq 'teste.vsix') { $digest = 'sha256:incorreto' }
            @{ name = $nome; state = 'uploaded'; size = $arquivo.Length; digest = $digest }
        }
        return (ConvertTo-Json -InputObject @(@{ tag_name = 'v9.9.9'; draft = $true; assets = @($assets) }) -Depth 8 -Compress)
    }
    if ($args[0] -eq 'release') { return }
    throw 'O teste recebeu um comando gh inesperado.'
}

function Assert-FalhaPublicacao {
    param([string]$Mensagem)
    $falhou = $false
    try { & $publicador -Tag v9.9.9 -DiretorioPacote $pasta -Commit $commit | Out-Null }
    catch {
        $falhou = $true
        if ($_.Exception.Message -notlike "*$Mensagem*") { throw }
    }
    if (-not $falhou) { throw "A publicação deveria falhar com: $Mensagem" }
    if (@($global:AndroidWifiReleaseTestState.Calls | Where-Object { $_ -like '*release edit*' }).Count -gt 0) {
        throw 'Uma publicação inválida tentou tornar a release pública.'
    }
}

try {
    [IO.Directory]::CreateDirectory($pasta) | Out-Null
    $env:GH_REPO = 'exemplo/repositorio'
    $hashes = foreach ($nome in @('teste.vsix', 'teste-devs.zip', 'teste-marketplace.zip')) {
        $arquivo = Join-Path $pasta $nome
        [IO.File]::WriteAllText($arquivo, "artefato de teste: $nome")
        '{0}  {1}' -f (Get-FileHash $arquivo -Algorithm SHA256).Hash.ToLowerInvariant(), $nome
    }
    [IO.File]::WriteAllLines((Join-Path $pasta 'SHA256SUMS.txt'), [string[]]$hashes)
    @{ versao = '9.9.8.0'; commit = $commit; preview = $true } | ConvertTo-Json | Set-Content (Join-Path $pasta 'release-info.json')
    Assert-FalhaPublicacao 'versão ou o commit'
    if ($global:AndroidWifiReleaseTestState.Calls.Count -ne 0) { throw 'Uma versão inválida chegou a consultar o GitHub.' }

    @{ versao = '9.9.9.0'; commit = $commit; preview = $true } | ConvertTo-Json | Set-Content (Join-Path $pasta 'release-info.json')
    $global:AndroidWifiReleaseTestState.Mode = 'tag-existente'
    $global:AndroidWifiReleaseTestState.Calls.Clear()
    Assert-FalhaPublicacao 'tag já existe'

    $global:AndroidWifiReleaseTestState.Mode = 'sucesso'
    $global:AndroidWifiReleaseTestState.Calls.Clear()
    [IO.File]::AppendAllText((Join-Path $pasta 'teste.vsix'), 'alterado')
    Assert-FalhaPublicacao 'Hash divergente'
    [IO.File]::WriteAllText((Join-Path $pasta 'teste.vsix'), 'artefato de teste: teste.vsix')

    $global:AndroidWifiReleaseTestState.Mode = 'upload-divergente'
    $global:AndroidWifiReleaseTestState.Calls.Clear()
    Assert-FalhaPublicacao 'Upload remoto não confirmado'

    $global:AndroidWifiReleaseTestState.Mode = 'sucesso'
    $global:AndroidWifiReleaseTestState.Calls.Clear()
    & $publicador -Tag v9.9.9 -DiretorioPacote $pasta -Commit $commit | Out-Null
    $criacao = @($global:AndroidWifiReleaseTestState.Calls | Where-Object { $_ -like 'release create*' })
    $publicacao = @($global:AndroidWifiReleaseTestState.Calls | Where-Object { $_ -like 'release edit*' })
    if ($criacao.Count -ne 1 -or $criacao[0] -notlike '*--draft*' -or $publicacao.Count -ne 1 -or $publicacao[0] -notlike '*--draft=false*') {
        throw 'A sequência rascunho, verificação e publicação não foi respeitada.'
    }
    Write-Output 'Cinco cenários do publicador aprovados com gh simulado e sem rede.'
}
finally {
    $env:GH_REPO = $repoAnterior
    Remove-Variable -Name AndroidWifiReleaseTestState -Scope Global -ErrorAction SilentlyContinue
    foreach ($arquivo in @(Get-ChildItem -LiteralPath $pasta -File -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $arquivo.FullName }
    if (Test-Path -LiteralPath $pasta) { Remove-Item -LiteralPath $pasta }
}
