param(
    [string]$PackageDirectory
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $PackageDirectory) {
    $PackageDirectory = Join-Path $root 'artifacts\package'
}

$infoPath = Join-Path $PackageDirectory 'info.ini'
$assemblyPath = Join-Path $PackageDirectory 'DuckovDownedRevive.dll'
$corePath = Join-Path $PackageDirectory 'DuckovDownedRevive.Core.dll'

foreach ($path in @($infoPath, $assemblyPath, $corePath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing package file: $path"
    }
}

$modNameLine = Get-Content -LiteralPath $infoPath |
    Where-Object { $_ -match '^\s*name\s*=' } |
    Select-Object -First 1
if (-not $modNameLine) {
    throw 'info.ini does not define name.'
}

$modName = ($modNameLine -split '=', 2)[1].Trim()
$expectedType = "$modName.ModBehaviour"
$expectedBase = 'Duckov.Modding.ModBehaviour'

$stream = [IO.File]::OpenRead($assemblyPath)
try {
    $peReader = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    $metadata = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader(
        $peReader)
    $entryType = $null

    foreach ($handle in $metadata.TypeDefinitions) {
        $definition = $metadata.GetTypeDefinition($handle)
        $namespace = $metadata.GetString($definition.Namespace)
        $name = $metadata.GetString($definition.Name)
        $fullName = if ($namespace) { "$namespace.$name" } else { $name }
        if ($fullName -eq $expectedType) {
            $entryType = $definition
            break
        }
    }

    if ($null -eq $entryType) {
        throw "Expected Mod entry type not found: $expectedType"
    }

    if ($entryType.BaseType.Kind -ne
        [System.Reflection.Metadata.HandleKind]::TypeReference) {
        throw "Unexpected base type kind for $expectedType."
    }

    $baseReference = $metadata.GetTypeReference(
        [System.Reflection.Metadata.TypeReferenceHandle]$entryType.BaseType)
    $baseNamespace = $metadata.GetString($baseReference.Namespace)
    $baseName = $metadata.GetString($baseReference.Name)
    $baseFullName = "$baseNamespace.$baseName"
    if ($baseFullName -ne $expectedBase) {
        throw "Expected $expectedType to inherit $expectedBase; found $baseFullName."
    }

    Write-Host "Package entry type verified: $expectedType"
} finally {
    $stream.Dispose()
}
