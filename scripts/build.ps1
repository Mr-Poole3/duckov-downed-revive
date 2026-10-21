param(
    [switch]$WarningsAsErrors,
    [switch]$SkipMod
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
$coreOutput = Join-Path $artifacts 'core'
$packageOutput = Join-Path $artifacts 'package'
$objOutput = Join-Path $artifacts 'obj'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path -LiteralPath $csc)) {
    throw "C# compiler not found at $csc"
}

New-Item -ItemType Directory -Path $coreOutput -Force | Out-Null
New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null
New-Item -ItemType Directory -Path $objOutput -Force | Out-Null

$coreSources = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'src\DuckovDownedRevive.Core') `
        -Filter '*.cs' -File |
        Select-Object -ExpandProperty FullName
)
$coreDll = Join-Path $coreOutput 'DuckovDownedRevive.Core.dll'
$coreArgs = @(
    '/nologo'
    '/target:library'
    '/optimize+'
    '/warn:4'
    "/out:$coreDll"
    '/reference:System.dll'
    '/reference:System.Core.dll'
)
if ($WarningsAsErrors) {
    $coreArgs += '/warnaserror+'
}
$coreArgs += $coreSources

& $csc @coreArgs
if ($LASTEXITCODE -ne 0) {
    throw 'Core build failed.'
}

if (-not $SkipMod) {
    $gameDir = 'D:\Games\Escape from Duckov'
    if ($env:DUCKOV_GAME_DIR) {
        $gameDir = $env:DUCKOV_GAME_DIR
    }

    $managed = Join-Path $gameDir 'Duckov_Data\Managed'
    $coop = Join-Path $gameDir 'Duckov_Data\Mods\3591341282'
    $liteNet = Join-Path $gameDir 'Duckov_Data\Mods\3591339491'
    $harmony = Join-Path $gameDir 'Duckov_Data\Mods\3589088839'
    $modDll = Join-Path $packageOutput 'DuckovDownedRevive.dll'
    $modSources = @(
        Get-ChildItem -LiteralPath (Join-Path $root 'src\DuckovDownedRevive.Mod') `
            -Filter '*.cs' -File -Recurse |
            Select-Object -ExpandProperty FullName
    )
    $modArgs = @(
        '/nologo'
        '/target:library'
        '/optimize+'
        '/warn:4'
        "/out:$modDll"
        '/reference:System.dll'
        '/reference:System.Core.dll'
        "/reference:$coreDll"
        "/reference:$(Join-Path $managed 'netstandard.dll')"
        "/reference:$(Join-Path $managed 'System.Runtime.dll')"
        "/reference:$(Join-Path $managed 'TeamSoda.Duckov.Core.dll')"
        "/reference:$(Join-Path $managed 'UnityEngine.dll')"
        "/reference:$(Join-Path $managed 'UnityEngine.CoreModule.dll')"
        "/reference:$(Join-Path $managed 'UnityEngine.IMGUIModule.dll')"
        "/reference:$(Join-Path $managed 'UnityEngine.InputLegacyModule.dll')"
        "/reference:$(Join-Path $coop 'EscapeFromDuckovModApi.dll')"
        "/reference:$(Join-Path $liteNet 'LiteNetLib.dll')"
        "/reference:$(Join-Path $harmony '0Harmony.dll')"
    )
    if ($WarningsAsErrors) {
        $modArgs += '/warnaserror+'
    }
    $modArgs += $modSources

    & $csc @modArgs
    if ($LASTEXITCODE -ne 0) {
        throw 'Mod build failed.'
    }

    Copy-Item -LiteralPath $coreDll `
        -Destination (Join-Path $packageOutput 'DuckovDownedRevive.Core.dll') -Force
    Copy-Item -LiteralPath (Join-Path $root 'mod\info.ini') `
        -Destination (Join-Path $packageOutput 'info.ini') -Force

    $pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($pwsh) {
        & $pwsh.Source -NoProfile -ExecutionPolicy Bypass `
            -File (Join-Path $root 'scripts\validate-package.ps1') `
            -PackageDirectory $packageOutput
        if ($LASTEXITCODE -ne 0) {
            throw 'Package validation failed.'
        }
    } else {
        Write-Warning 'pwsh is unavailable; package type validation was skipped.'
    }
}

Write-Host "Build complete: $artifacts"
