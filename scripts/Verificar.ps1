#requires -Version 7.2
param([ValidateSet('Debug', 'Release')][string]$Configuracao = 'Release')
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Push-Location $raiz
try {
    if (-not $IsWindows) { throw 'A validação completa requer Windows por causa do WPF e do Visual Studio.' }
    & (Join-Path $PSScriptRoot 'TestarRepositorio.ps1')
    & (Join-Path $PSScriptRoot 'TestarPublicacao.ps1')
    & dotnet restore DiscoveryDeviceAndroid.slnx --locked-mode -v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'A restauração de dependências falhou.' }
    & dotnet format whitespace DiscoveryDeviceAndroid.slnx --verify-no-changes --no-restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw 'A formatação difere do .editorconfig.' }
    & dotnet build DiscoveryDeviceAndroid.slnx -c $Configuracao --no-restore -v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'A compilação falhou.' }
    foreach ($projeto in @('DiscoveryDeviceAndroid.Tests', 'DiscoveryDeviceAndroid.UiTests')) {
        & dotnet test $projeto -c $Configuracao --no-build --no-restore -v:minimal --logger trx --results-directory (Join-Path $raiz "TestResults\$projeto")
        if ($LASTEXITCODE -ne 0) { throw "Os testes de $projeto falharam." }
    }
}
finally { Pop-Location }
