# ClickOnce Deployment Script for Together Application
# This script publishes the application using ClickOnce deployment

param(
    [string]$Configuration = "Release",
    [string]$PublishUrl = ".\publish\",
    [string]$Version = "1.0.0",
    [switch]$IncrementRevision
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Together Application - ClickOnce Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
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
Write-Host "Publish URL: $PublishUrl" -ForegroundColor Yellow
Write-Host "Version: $Version" -ForegroundColor Yellow
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

# Build the solution
Write-Host "Building solution..." -ForegroundColor Green
dotnet build $SolutionPath --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}

# Publish using ClickOnce
Write-Host "Publishing with ClickOnce..." -ForegroundColor Green

$PublishCommand = "msbuild $ProjectPath /t:Publish /p:Configuration=$Configuration /p:PublishUrl=$PublishUrl /p:ApplicationVersion=$Version"

if ($IncrementRevision) {
    $PublishCommand += " /p:ApplicationRevision=*"
}

Write-Host "Executing: $PublishCommand" -ForegroundColor Yellow
Invoke-Expression $PublishCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Publish failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Published files location: $PublishUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test the deployment by running setup.exe from the publish folder" -ForegroundColor White
Write-Host "2. Copy the publish folder to your distribution server" -ForegroundColor White
Write-Host "3. Users can install by running setup.exe" -ForegroundColor White
Write-Host ""
