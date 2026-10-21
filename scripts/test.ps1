param(
    [switch]$WarningsAsErrors
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root 'artifacts\test'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path -LiteralPath $csc)) {
    throw "C# compiler not found at $csc"
}

New-Item -ItemType Directory -Path $artifacts -Force | Out-Null

$coreSources = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'src\DuckovDownedRevive.Core') `
        -Filter '*.cs' -File |
        Select-Object -ExpandProperty FullName
)
$testSources = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'tests\DuckovDownedRevive.Tests') `
        -Filter '*.cs' -File |
        Select-Object -ExpandProperty FullName
)
$testExe = Join-Path $artifacts 'DuckovDownedRevive.Tests.exe'
$args = @(
    '/nologo'
    '/target:exe'
    '/optimize+'
    '/warn:4'
    "/out:$testExe"
    '/reference:System.dll'
    '/reference:System.Core.dll'
)
if ($WarningsAsErrors) {
    $args += '/warnaserror+'
}
$args += $coreSources
$args += $testSources

& $csc @args
if ($LASTEXITCODE -ne 0) {
    throw 'Test build failed.'
}

& $testExe
if ($LASTEXITCODE -ne 0) {
    throw 'Tests failed.'
}
