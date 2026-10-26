#requires -Version 7.2
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$arquivos = @(& git -C $raiz ls-files --cached --others --exclude-standard)
if ($LASTEXITCODE -ne 0) { throw 'Não foi possível enumerar os arquivos do repositório.' }
foreach ($relativo in ($arquivos | Sort-Object -Unique)) {
    $caminho = Join-Path $raiz $relativo
    if (-not (Test-Path -LiteralPath $caminho -PathType Leaf)) { continue }
    switch ([IO.Path]::GetExtension($caminho)) {
        '.json' { Get-Content -LiteralPath $caminho -Raw | ConvertFrom-Json | Out-Null }
        '.csproj' { [xml](Get-Content -LiteralPath $caminho -Raw) | Out-Null }
        '.props' { [xml](Get-Content -LiteralPath $caminho -Raw) | Out-Null }
        '.slnx' { [xml](Get-Content -LiteralPath $caminho -Raw) | Out-Null }
        '.xaml' { [xml](Get-Content -LiteralPath $caminho -Raw) | Out-Null }
        '.ps1' {
            $tokens = $null
            $erros = $null
            [Management.Automation.Language.Parser]::ParseFile($caminho, [ref]$tokens, [ref]$erros) | Out-Null
            if ($erros.Count -gt 0) { throw "PowerShell inválido em ${relativo}: $($erros.Message -join '; ')" }
        }
        '.md' {
            foreach ($match in [regex]::Matches([IO.File]::ReadAllText($caminho), '\]\(([^\s)]+)(?:\s+"[^"]*")?\)')) {
                $referencia = $match.Groups[1].Value.Trim('<','>')
                if ($referencia -match '^([a-z][a-z0-9+.-]*:|#|/)') { continue }
                $arquivo = [Uri]::UnescapeDataString(($referencia -split '#', 2)[0])
                if ($arquivo -and -not (Test-Path -LiteralPath (Join-Path (Split-Path -Parent $caminho) $arquivo))) {
                    throw "Link local inexistente em ${relativo}: $referencia"
                }
            }
        }
    }
}
Write-Output 'Estrutura JSON/XML, sintaxe PowerShell e links locais da documentação verificados.'
