# Standalone Deployment Script for Together Application
# This script creates a self-contained executable with embedded .NET runtime

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\output\",
    [string]$Runtime = "win-x64",
    [switch]$SingleFile,
    [switch]$ReadyToRun
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Together Application - Standalone Deploy" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Set paths
$ProjectPath = "..\Together\Together.csproj"
$SolutionPath = "..\Together.sln"

# Check if project exists
if (-not (Test-Path $ProjectPath)) {
    Write-Host "Error: Project file not found at $ProjectPath" -ForegroundColor Red
    exit 1
}

Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
Write-Host "Runtime: $Runtime" -ForegroundColor Yellow
Write-Host "Single File: $SingleFile" -ForegroundColor Yellow
Write-Host "Ready To Run: $ReadyToRun" -ForegroundColor Yellow
Write-Host ""

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Green
dotnet clean $SolutionPath --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Clean failed" -ForegroundColor Red
    exit 1
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Green
dotnet restore $SolutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Restore failed" -ForegroundColor Red
    exit 1
}

# Build publish command
$PublishArgs = @(
    "publish",
    $ProjectPath,
    "--configuration", $Configuration,
    "--runtime", $Runtime,
    "--self-contained", "true",
    "--output", $OutputPath
)

if ($SingleFile) {
    $PublishArgs += "-p:PublishSingleFile=true"
    $PublishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
    $PublishArgs += "-p:EnableCompressionInSingleFile=true"
}

if ($ReadyToRun) {
    $PublishArgs += "-p:PublishReadyToRun=true"
}

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Green
Write-Host "Command: dotnet $($PublishArgs -join ' ')" -ForegroundColor Yellow
Write-Host ""

& dotnet $PublishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Publish failed" -ForegroundColor Red
    exit 1
}

# Calculate output size
$OutputSize = (Get-ChildItem -Path $OutputPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Published files location: $OutputPath" -ForegroundColor Cyan
Write-Host "Total size: $([math]::Round($OutputSize, 2)) MB" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test the application by running Together.exe from the output folder" -ForegroundColor White
Write-Host "2. Create an installer using tools like Inno Setup or WiX" -ForegroundColor White
Write-Host "3. Distribute the installer to users" -ForegroundColor White
Write-Host ""
