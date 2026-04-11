<#
.SYNOPSIS
    Pushes NuGet (.nupkg) packages for a given version to NuGet.org.

.DESCRIPTION
    Locates all .nupkg and .snupkg files matching the specified version under the current
    directory tree, retrieves the NuGet.org API key from Windows Credential Manager
    (prompting and storing it on first run), and pushes the packages using `dotnet nuget push`.

    The API key is stored in Windows Credential Manager under the target name defined by
    $NugetCredName ("NugetOrgRsllcApiKey"). On the first run, you will be prompted to enter
    the key interactively; it is then saved so subsequent runs are fully automated.

.PARAMETER version
    The exact package version string to push (e.g. "1.2.3" or "2.0.0-preview.1").

.EXAMPLE
    # Basic usage — push all packages built for version 1.2.3
    .\UploadToNuget.ps1 -version "1.2.3"

.EXAMPLE
    # Pre-release version
    .\UploadToNuget.ps1 -version "2.0.0-preview.1"

.NOTES
    HOW TO PRE-STORE YOUR API KEY (so you are never prompted at runtime):

    Option A — Run this script once and type the key when prompted.
               It will be saved automatically to Windows Credential Manager.

    Option B — Store the key manually before running the script:

        # 1. Install the CredentialManager module if not already present
        Install-Module CredentialManager -Scope CurrentUser -Force

        # 2. Store the NuGet.org API key (replace the placeholder with your real key)
        #    Target name MUST match: "NugetOrgRsllcApiKey"
        New-StoredCredential `
            -Target   "NugetOrgRsllcApiKey" `
            -UserName "NuGetUser" `
            -Password "oy2aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" `
            -Persist  LocalMachine

    Option C — One-liner version of Option B
        New-StoredCredential -Target "NugetOrgRsllcApiKey" -UserName "NuGetUser" -Password "<YOUR_API_KEY>" -Persist LocalMachine

    You can obtain your API key from: https://www.nuget.org/account/apikeys
    Recommended key scopes: "Push new packages and package versions" scoped to your package prefix.
#>
param (
    [Parameter(Mandatory = $true)]
    [string]$version
)

# Ensure dotnet is available
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "The .NET SDK is not installed or not in PATH."
    exit 1
}

# Find packages
$nupkgFiles = Get-ChildItem -Recurse -Filter "*.$version.nupkg" -File |
    Where-Object { $_.Name -notlike "*.snupkg" }

$snupkgFiles = Get-ChildItem -Recurse -Filter "*.$version.snupkg" -File

if ($nupkgFiles.Count -eq 0 -and $snupkgFiles.Count -eq 0) {
    Write-Warning "No matching .nupkg or .snupkg files found for version '$version'."
    exit 1
}

# The Windows Credential Manager target name used to store/retrieve the NuGet.org API key.
# To pre-store the key, run:
#   New-StoredCredential -Target "NugetOrgRsllcApiKey" -UserName "NuGetUser" -Password "<YOUR_API_KEY>" -Persist LocalMachine
$NugetCredName = "NugetOrgRsllcApiKey"

function Get-NugetApiKey {
    try {
        # Attempt to load the API key from Windows Credential Manager
        $cred = Get-StoredCredential -Target $NugetCredName -ErrorAction Stop        
        if (-not $cred) {
            throw "Credential '$NugetCredName' not found in Windows Credential Manager."
        }
        # Decrypt the SecureString password back to plain text for use with dotnet nuget push
        return $([Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($cred.Password)))
    } catch {
        # First-run fallback: prompt the user for the key and persist it for future runs
        Write-Host "🔐 No API key found for nuget.org. Please enter your API key:"
        $apiKey = Read-Host -AsSecureString "NuGet.org API Key"
        $plaintextApiKey = [System.Net.NetworkCredential]::new("", $apiKey).Password

        # Persist to Windows Credential Manager so the user is not prompted again
        New-StoredCredential -Target $NugetCredName -UserName "NuGetUser" -Password $plaintextApiKey -Persist LocalMachine | Out-Null
        return $plaintextApiKey
    }
}

# Pushes a single .nupkg or .snupkg file to NuGet.org using the provided API key
function Push-Package($file, $apiKey) {
    Write-Host "📦 Pushing $($file.Name)..."
    dotnet nuget push $file.FullName `
        --source "https://api.nuget.org/v3/index.json" `
        --skip-duplicate `
        --api-key $apiKey
}

# Install CredentialManager module if not already available (required for Get/New-StoredCredential)
if (-not (Get-Module -ListAvailable -Name CredentialManager)) {
    Install-Module CredentialManager -Scope CurrentUser -Force
}

$apiKey = Get-NugetApiKey

Write-Host "`n🔍 Found the $NugetCredName api key"# => $apiKey"

$files = Get-ChildItem -Recurse -File | Where-Object { $_.Name -match "\.$Version\.(nupkg|snupkg)$" } | Sort-Object { $_.Name }

Write-Host "`n🔍 Found the following packages to push:"
$files | ForEach-Object { Write-Host "  $_" }

# Confirm push
$confirmation = Read-Host "`n🚀 Press Enter to push to NuGet.org, or Ctrl+C to cancel"

foreach ($file in $files | Where-Object { $_.Extension -ne ".snupkg" }) {
    Push-Package -file $file -apiKey $apiKey
}

Write-Host "`n✅ All done!"
