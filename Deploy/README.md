# Together Application - Deployment Guide

This directory contains scripts and configuration files for deploying the Together application.

## Deployment Options

Together supports two deployment methods:

1. **ClickOnce Deployment** - Automatic updates, easy installation
2. **Standalone Installer** - Traditional installer with self-contained .NET runtime

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (for ClickOnce) or MSBuild
- PowerShell 5.1 or later
- Inno Setup 6.0+ (for standalone installer creation)

## Option 1: ClickOnce Deployment

ClickOnce provides automatic update functionality and easy installation for users.

### Features

- Automatic update checks on application startup
- Incremental updates (only changed files are downloaded)
- No administrator privileges required for installation
- Automatic rollback on failed updates
- Version management

### Publishing with ClickOnce

1. **Using PowerShell Script:**

```powershell
cd Deploy
.\publish-clickonce.ps1 -Configuration Release -Version "1.0.0"
```

Parameters:
- `-Configuration`: Build configuration (Debug/Release)
- `-PublishUrl`: Output directory for published files
- `-Version`: Application version (e.g., "1.0.0")
- `-IncrementRevision`: Automatically increment revision number

2. **Using Visual Studio:**

- Right-click the Together project
- Select "Publish"
- Choose "ClickOnce" as the target
- Configure publish settings
- Click "Publish"

### Distributing ClickOnce Application

1. Copy the entire `publish` folder to a web server or network share
2. Users can install by navigating to the URL and clicking `setup.exe`
3. For updates, simply publish a new version to the same location

### Update Configuration

The application checks for updates:
- On startup (automatic for required updates)
- Manually via the Update menu in the application

Configure update settings in `Together.csproj`:
```xml
<UpdateEnabled>true</UpdateEnabled>
<UpdateMode>Foreground</UpdateMode>
<UpdateInterval>7</UpdateInterval>
<UpdateIntervalUnits>Days</UpdateIntervalUnits>
```

## Option 2: Standalone Installer

Creates a traditional Windows installer with embedded .NET runtime.

### Features

- No .NET runtime installation required
- Single executable option
- Full offline installation support
- Custom installer branding
- Registry integration

### Creating Standalone Build

1. **Publish the application:**

```powershell
cd Deploy
.\publish-standalone.ps1 -Configuration Release -Runtime win-x64 -SingleFile -ReadyToRun
```

Parameters:
- `-Configuration`: Build configuration (Debug/Release)
- `-OutputPath`: Output directory for published files
- `-Runtime`: Target runtime (win-x64, win-x86, win-arm64)
- `-SingleFile`: Create single executable file
- `-ReadyToRun`: Enable ahead-of-time compilation for faster startup

2. **Create installer with Inno Setup:**

```powershell
# Install Inno Setup from https://jrsoftware.org/isinfo.php
# Then compile the installer script
iscc create-installer.iss
```

The installer will be created in the `Deploy\installer` directory.

### Customizing the Installer

Edit `create-installer.iss` to customize:
- Application name and version
- Installation directory
- Desktop/Start menu shortcuts
- File associations
- Custom installation steps
- Uninstallation behavior

## Application Icon and Branding

### Adding Application Icon

1. Create or obtain an `.ico` file with multiple sizes (16x16, 32x32, 48x48, 256x256)
2. Place the icon file at `Together\Assets\together.ico`
3. The icon is automatically included via the project configuration

### Icon Resources

- Application executable icon
- Installer icon
- Desktop shortcut icon
- Taskbar icon

## Version Management

### Updating Version Numbers

Version numbers are managed in `Together.csproj`:

```xml
<Version>1.0.0</Version>
<AssemblyVersion>1.0.0.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
```

### Version Numbering Scheme

Follow semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

For ClickOnce, also update:
```xml
<ApplicationRevision>1</ApplicationRevision>
```

## Automatic Updates

### How It Works

1. Application checks for updates on startup
2. If update is available, user is notified
3. User can choose to install immediately or later
4. Required updates are installed automatically
5. Application restarts after update installation

### Update Service

The `UpdateService` class handles all update operations:

```csharp
// Check for updates
var result = await updateService.CheckForUpdateAsync();

// Install update
var installResult = await updateService.InstallUpdateAsync(progress);
```

### Update UI

Users can manually check for updates via:
- Help → Check for Updates menu
- Settings → Updates section

## Testing Deployment

### Testing ClickOnce

1. Publish to a local folder
2. Install from the publish folder
3. Modify the application and publish again with incremented version
4. Launch the installed application to test automatic update

### Testing Standalone Installer

1. Build the standalone package
2. Create the installer
3. Install on a clean test machine
4. Verify all features work correctly
5. Test uninstallation

## Troubleshooting

### ClickOnce Issues

**Problem**: Updates not detected
- Verify publish URL is accessible
- Check network connectivity
- Ensure version number was incremented

**Problem**: Installation fails
- Check certificate validity (if signed)
- Verify .NET Framework requirements
- Check Windows security settings

### Standalone Installer Issues

**Problem**: Application won't start
- Verify all dependencies are included
- Check for missing DLL files
- Review application logs

**Problem**: Large installer size
- Enable compression in Inno Setup
- Use single-file publishing
- Consider excluding debug symbols

## Security Considerations

### Code Signing

For production deployments, sign your application:

1. **Obtain a code signing certificate**
2. **Sign the executable:**

```powershell
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com Together.exe
```

3. **Sign the installer:**

```powershell
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com Together-Setup.exe
```

### ClickOnce Manifest Signing

Configure in Visual Studio:
- Project Properties → Signing
- Check "Sign the ClickOnce manifests"
- Select or create a certificate

## Distribution Checklist

Before distributing to users:

- [ ] Update version numbers
- [ ] Test on clean Windows installation
- [ ] Verify all features work correctly
- [ ] Check automatic updates (ClickOnce)
- [ ] Sign executables and installer
- [ ] Create release notes
- [ ] Update documentation
- [ ] Backup previous version
- [ ] Test uninstallation process
- [ ] Verify icon and branding
- [ ] Check file associations
- [ ] Test on different Windows versions

## Continuous Deployment

### Automated Build Pipeline

Example GitHub Actions workflow:

```yaml
name: Build and Deploy

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 8.0.x
      - name: Publish
        run: |
          cd Deploy
          .\publish-standalone.ps1 -Configuration Release
      - name: Create Release
        uses: actions/create-release@v1
        with:
          tag_name: ${{ github.ref }}
          release_name: Release ${{ github.ref }}
```

## Support

For deployment issues or questions:
- Check the troubleshooting section
- Review application logs
- Contact support team

## Additional Resources

- [ClickOnce Documentation](https://docs.microsoft.com/en-us/visualstudio/deployment/clickonce-security-and-deployment)
- [.NET Publishing Guide](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [Inno Setup Documentation](https://jrsoftware.org/ishelp/)
- [Code Signing Guide](https://docs.microsoft.com/en-us/windows/win32/seccrypto/cryptography-tools)
