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

$NugetCredName = "NugetOrgRsllcApiKey"

function Get-NugetApiKey {
    try {
        $cred = Get-StoredCredential -Target $NugetCredName -ErrorAction Stop        
        if (-not $cred) {
            throw "Credential '$NugetCredName' not found in Windows Credential Manager."
        }
        return $([Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($cred.Password)))
    } catch {
        Write-Host "🔐 No API key found for nuget.org. Please enter your API key:"
        $apiKey = Read-Host -AsSecureString "NuGet.org API Key"
        $plaintextApiKey = [System.Net.NetworkCredential]::new("", $apiKey).Password

        # Store it
        New-StoredCredential -Target $NugetCredName -UserName "NuGetUser" -Password $plaintextApiKey -Persist LocalMachine | Out-Null
        return $plaintextApiKey
    }
}

# Push packages
function Push-Package($file, $apiKey) {
    Write-Host "📦 Pushing $($file.Name)..."
    dotnet nuget push $file.FullName `
        --source "https://api.nuget.org/v3/index.json" `
        --skip-duplicate `
        --api-key $apiKey
}

# Requires CredentialManager module
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
