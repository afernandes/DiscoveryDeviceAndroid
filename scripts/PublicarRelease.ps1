#requires -Version 7.2
param(
    [Parameter(Mandatory)][ValidatePattern('^v\d+\.\d+\.\d+$')][string]$Tag,
    [Parameter(Mandatory)][string]$DiretorioPacote,
    [Parameter(Mandatory)][ValidatePattern('^[a-f0-9]{40}$')][string]$Commit
)
$ErrorActionPreference = 'Stop'
$pacote = [IO.Path]::GetFullPath($DiretorioPacote)
$info = Get-Content (Join-Path $pacote 'release-info.json') -Raw | ConvertFrom-Json
$versao = [version]$info.versao
if ($Tag -ne ('v{0}.{1}.{2}' -f $versao.Major, $versao.Minor, $versao.Build) -or $info.commit -ne $Commit) {
    throw 'A versão ou o commit do pacote não coincide com a publicação solicitada.'
}
$existentes = & gh api "repos/$env:GH_REPO/git/matching-refs/tags/$Tag" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Não foi possível consultar as tags remotas.' }
if (@($existentes | Where-Object ref -eq "refs/tags/$Tag").Count -gt 0) { throw 'A tag já existe. Releases anteriores não serão sobrescritas.' }
$arquivos = @()
foreach ($linha in [IO.File]::ReadAllLines((Join-Path $pacote 'SHA256SUMS.txt'))) {
    if ($linha -notmatch '^([a-f0-9]{64})  ([A-Za-z0-9._-]+)$') { throw 'Manifesto de hashes inválido.' }
    $esperado = $Matches[1]
    $nome = $Matches[2]
    $caminho = Join-Path $pacote $nome
    if ((Get-FileHash -LiteralPath $caminho -Algorithm SHA256).Hash.ToLowerInvariant() -ne $esperado) { throw "Hash divergente: $nome" }
    $arquivos += $caminho
}
if ($arquivos.Count -ne 3 -or @($arquivos | Where-Object { [IO.Path]::GetExtension($_) -eq '.vsix' }).Count -ne 1) {
    throw 'São esperados um VSIX e dois ZIPs validados.'
}
$arquivos += Join-Path $pacote 'SHA256SUMS.txt'
$notas = Join-Path ([IO.Path]::GetTempPath()) ('android-wifi-release-' + [guid]::NewGuid().ToString('N') + '.md')
try {
    @"
# Android via Wi-Fi $Tag

Build e testes aprovados no GitHub Actions para o commit $Commit.

- O VSIX instala a extensão; o ZIP com sufixo devs inclui instruções e licença MIT.
- O ZIP marketplace reúne os materiais de submissão; esta publicação não envia a extensão à loja.
- Confira SHA256SUMS.txt antes de instalar.

Compatibilidade declarada: Windows x64, Visual Studio 2022 17.14 e Visual Studio 2026, Android 11+ e Android SDK Platform-Tools.

A versão é Preview quando marcada como pré-lançamento. Testes automatizados não substituem a validação do pareamento físico e do IDE. Consulte o CHANGELOG e a matriz de validação no repositório.
"@ | Set-Content -LiteralPath $notas -Encoding utf8
    $opcoes = @('release', 'create', $Tag, '--target', $Commit, '--title', "Android via Wi-Fi $Tag", '--notes-file', $notas, '--draft')
    if ($info.preview) { $opcoes += '--prerelease' }
    & gh @opcoes @arquivos
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao criar o rascunho. Nenhuma tentativa automática de sobrescrita será feita.' }
    $releases = & gh api "repos/$env:GH_REPO/releases" | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao consultar a release criada.' }
    $release = @($releases | Where-Object tag_name -eq $Tag)
    if ($release.Count -ne 1 -or -not $release[0].draft) { throw 'Rascunho não encontrado.' }
    foreach ($arquivo in $arquivos) {
        $local = Get-Item -LiteralPath $arquivo
        $remoto = @($release[0].assets | Where-Object name -eq $local.Name)
        $hash = (Get-FileHash -LiteralPath $arquivo -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($remoto.Count -ne 1 -or $remoto[0].state -ne 'uploaded' -or $remoto[0].size -ne $local.Length -or $remoto[0].digest -ne "sha256:$hash") {
            throw "Upload remoto não confirmado: $($local.Name). O rascunho foi mantido."
        }
    }
    $opcoes = @('release', 'edit', $Tag, '--draft=false')
    if ($info.preview) { $opcoes += @('--prerelease', '--latest=false') }
    & gh @opcoes
    if ($LASTEXITCODE -ne 0) { throw 'Os arquivos estão no rascunho, mas a publicação falhou.' }
}
finally {
    if (Test-Path -LiteralPath $notas) { Remove-Item -LiteralPath $notas }
}
