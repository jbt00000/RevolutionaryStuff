<#
.SYNOPSIS
    Synchronizes selected filesystem trees into solution folders in a .slnx file.

.DESCRIPTION
    .slnx currently requires explicit <File> entries for solution items. This script
    maintains selected solution folders automatically.

    Configure:
      1. $SolutionFile
      2. $SolutionFolderMappings

    Each mapping associates a Solution Explorer folder with a filesystem path to
    traverse recursively. The script replaces only those managed solution folders
    and leaves all other solution content intact.

    Run this script from any directory. Paths are resolved relative to the script
    location unless they are absolute.

.NOTES
    Intended for repository-level folders such as:
      - docs
      - .github instructions
      - .github workflows/actions
      - infrastructure / IaC

    Requires PowerShell 7+.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# -----------------------------------------------------------------------------
# Configuration
# -----------------------------------------------------------------------------

# Path to the .slnx file, relative to this script unless absolute.
$SolutionFile = 'RevolutionaryStuff.slnx'

# Solution folder name => filesystem path.
#
# Examples below assume this script lives in the repository root.
# Change names/paths as appropriate for the repository.
#
# Notes:
# - Solution folder names do not need to match physical directory names.
# - Paths are traversed recursively.
# - Empty directories are not represented because .slnx solution folders contain
#   files, not physical-directory declarations.
# - Nested filesystem directories are represented as nested <Folder> elements.
$SolutionFolderMappings = [ordered]@{
    '/_content/docs/'             = 'docs'
    '/_content/GitHub/Instructions/' = '.github/instructions'
    '/_content/GitHub/Workflows/' = '.github/workflows'
#    '/_content/Infra/'              = 'inf'
}

# Optional file exclusions. Paths are matched against repository-relative paths.
# Add entries as needed.
$ExcludedFilePatterns = @(
    '*.user',
    '*.suo',
    '*.tmp',
    '*.bak'
)

# Directory names that should not be traversed at any depth.
# Matching is case-insensitive.
$ExcludedDirectoryNames = @(
    'bin',
    'obj'
)

# -----------------------------------------------------------------------------
# Helpers
# -----------------------------------------------------------------------------

function Resolve-ConfiguredPath {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $BaseDirectory
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BaseDirectory $Path))
}

function Normalize-SolutionFolderName {
    param(
        [Parameter(Mandatory)]
        [string] $Name
    )

    $normalized = $Name.Replace('\', '/').Trim()

    while ($normalized.Contains('//')) {
        $normalized = $normalized.Replace('//', '/')
    }

    if (-not $normalized.StartsWith('/')) {
        $normalized = '/' + $normalized
    }

    if (-not $normalized.EndsWith('/')) {
        $normalized += '/'
    }

    return $normalized
}

function Join-SolutionFolderPath {
    param(
        [Parameter(Mandatory)]
        [string] $Root,

        [AllowEmptyString()]
        [string] $RelativeDirectory
    )

    $rootNormalized = Normalize-SolutionFolderName -Name $Root

    if ([string]::IsNullOrWhiteSpace($RelativeDirectory) -or
        $RelativeDirectory -eq '.') {
        return $rootNormalized
    }

    $relativeNormalized = $RelativeDirectory.Replace('\', '/').Trim('/')

    return Normalize-SolutionFolderName -Name (
        $rootNormalized.TrimEnd('/') + '/' + $relativeNormalized
    )
}

function Get-RelativePathWithForwardSlashes {
    param(
        [Parameter(Mandatory)]
        [string] $BasePath,

        [Parameter(Mandatory)]
        [string] $TargetPath
    )

    return [System.IO.Path]::GetRelativePath($BasePath, $TargetPath).Replace('\', '/')
}

function Test-IsExcludedFile {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath
    )

    $leafName = Split-Path $RelativePath -Leaf

    foreach ($pattern in $ExcludedFilePatterns) {
        if ($RelativePath -like $pattern -or $leafName -like $pattern) {
            return $true
        }
    }

    return $false
}

function Remove-ManagedSolutionFolders {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $SolutionElement,

        [Parameter(Mandatory)]
        [string] $RootSolutionFolder
    )

    $root = Normalize-SolutionFolderName -Name $RootSolutionFolder
    $removedCount = 0

    # Snapshot all Folder descendants. Valid .slnx files keep Folder elements
    # directly under <Solution>, but this also finds/removes invalid nested folders
    # produced by older versions of this script.
    $folders = @($SolutionElement.SelectNodes('.//Folder'))

    # Remove deepest nodes first so cleanup also works for old invalid nesting.
    [array]::Reverse($folders)

    foreach ($folder in $folders) {
        $name = Normalize-SolutionFolderName -Name $folder.GetAttribute('Name')

        if ($name.Equals($root, [System.StringComparison]::OrdinalIgnoreCase) -or
            $name.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {

            if ($null -ne $folder.ParentNode) {
                [void]$folder.ParentNode.RemoveChild($folder)
                $removedCount++
            }
        }
    }

    return $removedCount
}

function Test-IsExcludedDirectory {
    param(
        [Parameter(Mandatory)]
        [string] $DirectoryName
    )

    foreach ($excludedName in $ExcludedDirectoryNames) {
        if ($DirectoryName.Equals(
            $excludedName,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
            return $true
        }
    }

    return $false
}

function Get-FilesRecursively {
    param(
        [Parameter(Mandatory)]
        [string] $DirectoryPath
    )

    foreach ($file in (Get-ChildItem -LiteralPath $DirectoryPath -File | Sort-Object FullName)) {
        $file
    }

    foreach ($directory in (Get-ChildItem -LiteralPath $DirectoryPath -Directory | Sort-Object FullName)) {
        if (Test-IsExcludedDirectory -DirectoryName $directory.Name) {
            Write-Host "  Skipping directory: $($directory.FullName)"
            continue
        }

        Get-FilesRecursively -DirectoryPath $directory.FullName
    }
}

function Get-FilesGroupedBySolutionFolder {
    param(
        [Parameter(Mandatory)]
        [string] $PhysicalRoot,

        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $RootSolutionFolder
    )

    $groups = [ordered]@{}

    if (-not (Test-Path -LiteralPath $PhysicalRoot -PathType Container)) {
        Write-Warning "Mapped path does not exist: $PhysicalRoot"
        return $groups
    }

    $files = Get-FilesRecursively -DirectoryPath $PhysicalRoot

    foreach ($file in $files) {
        $repoRelativePath = Get-RelativePathWithForwardSlashes `
            -BasePath $RepositoryRoot `
            -TargetPath $file.FullName

        if (Test-IsExcludedFile -RelativePath $repoRelativePath) {
            continue
        }

        $physicalRelativePath = [System.IO.Path]::GetRelativePath(
            $PhysicalRoot,
            $file.FullName
        )

        $relativeDirectory = Split-Path $physicalRelativePath -Parent

        $solutionFolder = Join-SolutionFolderPath `
            -Root $RootSolutionFolder `
            -RelativeDirectory $relativeDirectory

        if (-not $groups.Contains($solutionFolder)) {
            $groups[$solutionFolder] = [System.Collections.Generic.List[string]]::new()
        }

        $groups[$solutionFolder].Add($repoRelativePath)
    }

    return $groups
}

function Add-SolutionFolder {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlElement] $SolutionElement,

        [Parameter(Mandatory)]
        [System.Xml.XmlDocument] $Document,

        [Parameter(Mandatory)]
        [string] $FolderName,

        [Parameter(Mandatory)]
        [System.Collections.Generic.IEnumerable[string]] $FilePaths
    )

    $folder = $Document.CreateElement('Folder')
    $folder.SetAttribute(
        'Name',
        (Normalize-SolutionFolderName -Name $FolderName)
    )

    foreach ($filePath in ($FilePaths | Sort-Object)) {
        $fileElement = $Document.CreateElement('File')
        $fileElement.SetAttribute('Path', $filePath)
        [void]$folder.AppendChild($fileElement)
    }

    [void]$SolutionElement.AppendChild($folder)
}

# -----------------------------------------------------------------------------
# Main
# -----------------------------------------------------------------------------

$scriptRoot = $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($scriptRoot)) {
    $scriptRoot = (Get-Location).Path
}

$solutionPath = Resolve-ConfiguredPath `
    -Path $SolutionFile `
    -BaseDirectory $scriptRoot

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Solution file not found: $solutionPath"
}

$repositoryRoot = Split-Path $solutionPath -Parent

[xml] $document = Get-Content -LiteralPath $solutionPath -Raw

if ($null -eq $document.Solution) {
    throw "The file does not contain a <Solution> root element: $solutionPath"
}

$solutionElement = [System.Xml.XmlElement] $document.Solution

# Validate mappings after normalization and reject overlapping managed roots.
$managedRoots = [System.Collections.Generic.List[string]]::new()

foreach ($mapping in $SolutionFolderMappings.GetEnumerator()) {
    $root = Normalize-SolutionFolderName -Name ([string] $mapping.Key)

    foreach ($existingRoot in $managedRoots) {
        if ($root.Equals($existingRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
            $root.StartsWith($existingRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
            $existingRoot.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {

            throw "Managed solution folder mappings overlap: '$root' and '$existingRoot'"
        }
    }

    $managedRoots.Add($root)
}

foreach ($mapping in $SolutionFolderMappings.GetEnumerator()) {
    $rootSolutionFolder = Normalize-SolutionFolderName -Name ([string] $mapping.Key)

    $physicalRoot = Resolve-ConfiguredPath `
        -Path ([string] $mapping.Value) `
        -BaseDirectory $repositoryRoot

    Write-Host "Syncing $rootSolutionFolder <= $physicalRoot"

    $removedCount = Remove-ManagedSolutionFolders `
        -SolutionElement $solutionElement `
        -RootSolutionFolder $rootSolutionFolder

    Write-Host "  Removed $removedCount existing managed folder node(s)."

    $groups = Get-FilesGroupedBySolutionFolder `
        -PhysicalRoot $physicalRoot `
        -RepositoryRoot $repositoryRoot `
        -RootSolutionFolder $rootSolutionFolder

    foreach ($folderName in ($groups.Keys | Sort-Object)) {
        Add-SolutionFolder `
            -SolutionElement $solutionElement `
            -Document $document `
            -FolderName $folderName `
            -FilePaths $groups[$folderName]
    }

    Write-Host "  Added $($groups.Count) solution folder node(s)."
}

# Write stable, readable UTF-8 without BOM.
$settings = [System.Xml.XmlWriterSettings]::new()
$settings.Indent = $true
$settings.IndentChars = '  '
$settings.NewLineChars = [Environment]::NewLine
$settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
$settings.OmitXmlDeclaration = $true
$settings.Encoding = [System.Text.UTF8Encoding]::new($false)

$writer = [System.Xml.XmlWriter]::Create($solutionPath, $settings)

try {
    $document.Save($writer)
}
finally {
    $writer.Dispose()
}

Write-Host "Updated solution: $solutionPath"
