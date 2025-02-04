# Define paths
$NugetPath = "nuget"
$MSBuildPath = "msbuild"
$SolutionPath = "$PSScriptRoot\GA-SDK-UWP.sln"
$OutputBaseDir = "$PSScriptRoot\BuildOutput\"

# Restore NuGet packages
& $NugetPath restore $SolutionPath

# Ensure output directory exists
New-Item -ItemType Directory -Path $OutputBaseDir -Force | Out-Null

# Function to build for a specific platform
function Build-UWP {
    param (
        [string]$Platform
    )
    $OutputDir = "$OutputBaseDir$Platform\"
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    & $MSBuildPath $SolutionPath /m /t:GA_SDK_UWP /p:Configuration=Release /p:Platform=$Platform  # /p:OutDir=$OutputDir
}

# Build for all architectures
Build-UWP -Platform "x86"
Build-UWP -Platform "x64"
Build-UWP -Platform "ARM"
Build-UWP -Platform "ARM64"