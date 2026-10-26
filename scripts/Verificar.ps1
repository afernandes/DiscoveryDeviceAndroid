param([ValidateSet('Debug', 'Release')][string]$Configuracao = 'Release')
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Push-Location $raiz
try {
    & dotnet build DiscoveryDeviceAndroid.slnx -c $Configuracao -v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'A compilação falhou.' }
    foreach ($projeto in @('DiscoveryDeviceAndroid.Tests', 'DiscoveryDeviceAndroid.UiTests')) {
        & dotnet test $projeto -c $Configuracao --no-build -v:minimal
        if ($LASTEXITCODE -ne 0) { throw "Os testes de $projeto falharam." }
    }
}
finally { Pop-Location }
