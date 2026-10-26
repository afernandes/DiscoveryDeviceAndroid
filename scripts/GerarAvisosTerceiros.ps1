$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$intermediario = Join-Path $raiz 'DiscoveryDeviceAndroid\obj\Release\net8.0-windows8.0'
$assets = Get-Content (Join-Path $raiz 'DiscoveryDeviceAndroid\obj\project.assets.json') -Raw | ConvertFrom-Json
$arquivos = (Get-Content (Join-Path $intermediario 'files.json') -Raw | ConvertFrom-Json).files
$pacotes = @{}
foreach ($arquivo in $arquivos) {
    foreach ($pasta in $assets.packageFolders.PSObject.Properties.Name) {
        if ($arquivo.path.StartsWith($pasta, [StringComparison]::OrdinalIgnoreCase)) {
            $partes = $arquivo.path.Substring($pasta.Length).Split([char[]]@('\','/'), [StringSplitOptions]::RemoveEmptyEntries)
            if ($partes.Length -ge 2) { $pacotes[($partes[0] + '/' + $partes[1])] = Join-Path (Join-Path $pasta $partes[0]) $partes[1] }
        }
    }
}
if ($pacotes.Count -eq 0) { throw 'Nenhum pacote de terceiros identificado nos arquivos do VSIX.' }
$texto = [Text.StringBuilder]::new()
[void]$texto.AppendLine('AVISOS E LICENÇAS DE COMPONENTES DE TERCEIROS')
[void]$texto.AppendLine('Inventário obtido dos arquivos incluídos no VSIX. As licenças dos componentes continuam aplicáveis independentemente da licença da extensão.')
foreach ($id in ($pacotes.Keys | Sort-Object)) {
    $pasta = $pacotes[$id]
    $nuspec = Get-ChildItem -LiteralPath $pasta -Filter '*.nuspec' | Select-Object -First 1
    [xml]$metadados = Get-Content -LiteralPath $nuspec.FullName -Raw
    [void]$texto.AppendLine()
    [void]$texto.AppendLine(('=' * 72))
    [void]$texto.AppendLine($id)
    [void]$texto.AppendLine('Autores: ' + $metadados.package.metadata.authors)
    [void]$texto.AppendLine('Copyright: ' + $metadados.package.metadata.copyright)
    [void]$texto.AppendLine('Licença declarada: ' + $metadados.package.metadata.license.InnerText)
    [void]$texto.AppendLine('URL: ' + $metadados.package.metadata.licenseUrl)
    $avisos = @(Get-ChildItem -LiteralPath $pasta -File | Where-Object { $_.Name -match '^(LICENSE|LICENCE|NOTICE|THIRD.PARTY.NOTICES)(\..*)?$' })
    if ($metadados.package.metadata.license.type -eq 'file') {
        $licenca = Join-Path $pasta $metadados.package.metadata.license.InnerText
        if (Test-Path -LiteralPath $licenca -PathType Leaf) { $avisos += Get-Item -LiteralPath $licenca }
    }
    foreach ($aviso in ($avisos | Sort-Object FullName -Unique)) {
        [void]$texto.AppendLine()
        [void]$texto.AppendLine('--- ' + $aviso.Name + ' ---')
        [void]$texto.AppendLine([IO.File]::ReadAllText($aviso.FullName))
    }
}
[IO.File]::WriteAllText((Join-Path $raiz 'DiscoveryDeviceAndroid\Distribution\THIRD-PARTY-NOTICES.txt'), $texto.ToString())
Write-Output ('Avisos gerados para {0} pacotes incluídos.' -f $pacotes.Count)
