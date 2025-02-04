# Define paths
$MSBuildPath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$SolutionPath = "$PSScriptRoot\GA-SDK-UWP.sln"
$OutputBaseDir = "$PSScriptRoot\BuildOutput\"

# Ensure output directory exists
New-Item -ItemType Directory -Path $OutputBaseDir -Force | Out-Null

# Function to build for a specific platform
function Build-UWP {
    param (
        [string]$Platform
    )
    $OutputDir = "$OutputBaseDir$Platform\"
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    & $MSBuildPath $SolutionPath /m /t:GA_SDK_UWP /p:Configuration=Release /p:Platform=$Platform /p:OutDir=$OutputDir
}

# Build for all architectures
Build-UWP -Platform "x86"
Build-UWP -Platform "x64"
Build-UWP -Platform "ARM"
Build-UWP -Platform "ARM64"