# Together Application - Deployment Quick Start

This guide will help you deploy the Together application in under 10 minutes.

## Prerequisites

Before you begin, ensure you have:
- [ ] .NET 8.0 SDK installed
- [ ] PowerShell 5.1 or later
- [ ] Application icon at `Together\Assets\together.ico` (or use placeholder)

## Quick Deployment Steps

### Method 1: ClickOnce (Recommended for Easy Updates)

**Step 1: Navigate to Deploy folder**
```powershell
cd Deploy
```

**Step 2: Run the publish script**
```powershell
.\publish-clickonce.ps1 -Configuration Release -Version "1.0.0"
```

**Step 3: Test the deployment**
```powershell
cd publish
.\setup.exe
```

**Step 4: Distribute**
- Copy the entire `publish` folder to your distribution location
- Users can install by running `setup.exe`

**Done!** ✅ Your application is deployed with automatic update support.

---

### Method 2: Standalone Installer (Traditional Installation)

**Step 1: Navigate to Deploy folder**
```powershell
cd Deploy
```

**Step 2: Publish the application**
```powershell
.\publish-standalone.ps1 -Configuration Release -SingleFile -ReadyToRun
```

**Step 3: Create installer (requires Inno Setup)**
```powershell
# Download and install Inno Setup from https://jrsoftware.org/isinfo.php
# Then run:
iscc create-installer.iss
```

**Step 4: Test the installer**
```powershell
cd installer
.\Together-Setup-1.0.0.exe
```

**Step 5: Distribute**
- Share the installer file with users
- Users run the installer to install the application

**Done!** ✅ Your application is packaged as a traditional Windows installer.

---

## What's Included

### Automatic Updates (ClickOnce Only)
- Application checks for updates on startup
- Users can manually check via Help → Check for Updates
- Required updates install automatically
- Optional updates prompt the user

### Application Features
- Self-contained .NET runtime (no installation required)
- Application icon and branding
- Desktop shortcut creation
- Start menu integration
- Proper uninstallation support

## Updating Your Application

### For ClickOnce:
1. Increment version number in `Together.csproj`
2. Run publish script again with new version
3. Upload to same location
4. Users automatically get notified of update

### For Standalone:
1. Increment version number in `Together.csproj`
2. Rebuild and create new installer
3. Distribute new installer to users
4. Users manually install update

## Troubleshooting

### "Application icon not found"
- Create or add an icon file at `Together\Assets\together.ico`
- Or comment out the `<ApplicationIcon>` line in `Together.csproj`

### "Publish failed"
- Ensure all projects build successfully first
- Check that all dependencies are restored
- Verify .NET 8.0 SDK is installed

### "Installer creation failed"
- Install Inno Setup from https://jrsoftware.org/isinfo.php
- Verify the output path in `create-installer.iss` matches your publish output
- Check that LICENSE.txt and README.txt exist (or remove from script)

### "Application won't start after installation"
- Verify all dependencies are included in publish
- Check Windows Event Viewer for error details
- Ensure target machine meets system requirements

## Next Steps

For more detailed information:
- Read `README.md` for comprehensive deployment guide
- Review `DEPLOYMENT_CHECKLIST.md` before production deployment
- Check application logs in `%APPDATA%\Together\logs`

## Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review the full deployment guide in `README.md`
3. Check application logs for error details
4. Contact the development team with error details

---

**Congratulations!** 🎉 You've successfully deployed the Together application.
