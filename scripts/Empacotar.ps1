param([string]$DiretorioSaida)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Push-Location $raiz
try {
    & (Join-Path $PSScriptRoot 'Verificar.ps1')
    & (Join-Path $PSScriptRoot 'GerarAvisosTerceiros.ps1')
    & dotnet build DiscoveryDeviceAndroid\DiscoveryDeviceAndroid.csproj -c Release --no-restore -v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao gerar o VSIX com os avisos de terceiros.' }

    $vsixOrigem = Join-Path $raiz 'DiscoveryDeviceAndroid\bin\Release\net8.0-windows8.0\DiscoveryDeviceAndroid.vsix'
    $pacote = [IO.Compression.ZipFile]::OpenRead($vsixOrigem)
    try {
        $entrada = $pacote.GetEntry('extension.vsixmanifest')
        if ($null -eq $entrada) { throw 'Manifesto ausente no VSIX.' }
        $leitor = [IO.StreamReader]::new($entrada.Open())
        try { [xml]$manifesto = $leitor.ReadToEnd() } finally { $leitor.Dispose() }
        foreach ($nome in @('DiscoveryDeviceAndroid.dll','DiscoveryDeviceAndroid.Core.dll','QRCoder.dll','Distribution/icon.png','Distribution/preview.png','Distribution/RELEASE-NOTES.txt','Distribution/LICENSE.txt','Distribution/THIRD-PARTY-NOTICES.txt','.vsextension/extension.json','.vsextension/string-resources.json')) {
            if ($null -eq $pacote.GetEntry($nome)) { throw "Arquivo obrigatório ausente no VSIX: $nome" }
        }
        $identidade = $manifesto.PackageManifest.Metadata.Identity
        if ($manifesto.PackageManifest.Metadata.License.Replace('\', '/') -ne 'Distribution/LICENSE.txt') { throw 'O manifesto do VSIX não referencia a licença da extensão.' }
        $versaoCompleta = [version]$identidade.Version
        $versao = '{0}.{1}.{2}' -f $versaoCompleta.Major, $versaoCompleta.Minor, $versaoCompleta.Build
        $nomeBase = "AndroidWifi-$versao-preview"
        $publisher = (Get-Content marketplace\publishmanifest.json -Raw | ConvertFrom-Json).publisher
        if ($publisher -ne $identidade.Publisher) { throw 'Publisher do manifesto da loja difere do publisher do VSIX.' }
    }
    finally { $pacote.Dispose() }

    if ([string]::IsNullOrWhiteSpace($DiretorioSaida)) { $DiretorioSaida = Join-Path $raiz ('dist\' + $nomeBase) }
    $destino = [IO.Path]::GetFullPath($DiretorioSaida)
    if (Test-Path -LiteralPath $destino) { throw "O destino já existe. Informe outro -DiretorioSaida para preservar o pacote anterior: $destino" }
    [IO.Directory]::CreateDirectory($destino) | Out-Null
    $compartilhar = Join-Path $destino 'Compartilhar'
    $publicar = Join-Path $destino 'Marketplace'
    [IO.Directory]::CreateDirectory($compartilhar) | Out-Null
    [IO.Directory]::CreateDirectory((Join-Path $publicar 'marketplace')) | Out-Null
    $vsix = Join-Path $destino ($nomeBase + '.vsix')
    Copy-Item -LiteralPath $vsixOrigem -Destination $vsix
    Copy-Item -LiteralPath $vsix -Destination $compartilhar
    Copy-Item -LiteralPath $vsix -Destination $publicar
    foreach ($pasta in @($compartilhar, $publicar)) {
        Copy-Item -LiteralPath (Join-Path $raiz 'docs\INSTALACAO.md') -Destination $pasta
        Copy-Item -LiteralPath (Join-Path $raiz 'DiscoveryDeviceAndroid\Distribution\RELEASE-NOTES.txt') -Destination $pasta
        Copy-Item -LiteralPath (Join-Path $raiz 'DiscoveryDeviceAndroid\Distribution\THIRD-PARTY-NOTICES.txt') -Destination $pasta
        Copy-Item -LiteralPath (Join-Path $raiz 'marketplace\PRIVACIDADE.md') -Destination $pasta
        Copy-Item -LiteralPath (Join-Path $raiz 'LICENSE.txt') -Destination (Join-Path $pasta 'LICENSE.txt')
        $hashVsix = (Get-FileHash -LiteralPath $vsix -Algorithm SHA256).Hash.ToLowerInvariant()
        [IO.File]::WriteAllText((Join-Path $pasta 'SHA256SUMS.txt'), "$hashVsix  $nomeBase.vsix" + [Environment]::NewLine)
    }
    Copy-Item -Path (Join-Path $raiz 'marketplace\*') -Destination (Join-Path $publicar 'marketplace')
    Copy-Item -LiteralPath (Join-Path $raiz 'DiscoveryDeviceAndroid\Distribution\icon.png') -Destination (Join-Path $publicar 'marketplace')
    Copy-Item -LiteralPath (Join-Path $raiz 'DiscoveryDeviceAndroid\Distribution\preview.png') -Destination (Join-Path $publicar 'marketplace')
    Copy-Item -LiteralPath (Join-Path $raiz 'docs\PUBLICACAO.md') -Destination $publicar
    $zipDevs = Join-Path $destino ($nomeBase + '-devs.zip')
    $zipLoja = Join-Path $destino ($nomeBase + '-marketplace.zip')
    [IO.Compression.ZipFile]::CreateFromDirectory($compartilhar, $zipDevs)
    [IO.Compression.ZipFile]::CreateFromDirectory($publicar, $zipLoja)
    $hashes = foreach ($arquivo in @($vsix, $zipDevs, $zipLoja)) {
        '{0}  {1}' -f (Get-FileHash -LiteralPath $arquivo -Algorithm SHA256).Hash.ToLowerInvariant(), [IO.Path]::GetFileName($arquivo)
    }
    [IO.File]::WriteAllLines((Join-Path $destino 'SHA256SUMS.txt'), [string[]]$hashes)
    [ordered]@{
        nome = 'Android via Wi-Fi'; versao = $identidade.Version; publisher = $publisher
        idVsix = $identidade.Id; geradoEmUtc = [DateTimeOffset]::UtcNow.ToString('O')
        assinaturaDoAutor = $false; publicado = $false
        licencaDefinida = $true; licenca = 'MIT'
    } | ConvertTo-Json | Set-Content (Join-Path $destino 'release-info.json') -Encoding utf8
    Write-Output "Pacote para os DEVs: $zipDevs"
    Write-Output "VSIX para instalação/upload: $vsix"
    Write-Output "Materiais do Marketplace: $zipLoja"
}
finally { Pop-Location }
