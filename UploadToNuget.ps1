param(
    [string]$Version,
    [string]$NuGetSource = "https://api.nuget.org/v3/index.json"
)

# Ensure the NuGet CLI is installed
if (-not (Get-Command "nuget.exe" -ErrorAction SilentlyContinue)) {
    throw "NuGet CLI is not installed. Please install it from https://www.nuget.org/downloads."
}

# Validate parameters
if (-not $Version) {
    throw "You must provide a filename version."
}

#az login

# Find all matching files
$matchingFiles = Get-ChildItem -Recurse -Include *$Version*.nupkg, *$Version*.snupkg

Write-Host "Looking for *$($Version)*.nupkg and *$($Version)*.snupkg"

foreach ($file in $matchingFiles) {
    # Upload each matching file to NuGet
    Write-Host "Uploading $($file.FullName)..."
    nuget push "$($file.FullName)" -Source $NuGetSource 
    #nuget push "$($file.FullName)" -Source $NuGetSource -NonInteractive
}
