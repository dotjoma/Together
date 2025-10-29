# Application Assets

## Application Icon

The `together.ico` file should be placed in this directory. This icon will be used for:
- Application executable icon
- Taskbar icon
- Desktop shortcut icon
- ClickOnce deployment icon
- Installer icon

### Icon Requirements

- **Format:** ICO (Windows Icon)
- **Required sizes:** 16x16, 32x32, 48x48, 256x256 pixels
- **Color depth:** 32-bit with alpha channel (transparency support)
- **File name:** `together.ico` (exact name required)
- **Location:** `Together/Assets/together.ico`

### Design Guidelines

The icon should:
- Represent the Together brand and application purpose
- Be simple and recognizable at small sizes (16x16)
- Use colors that match the application theme
- Work well on both light and dark backgrounds
- Convey emotional connection, relationships, or togetherness

**Suggested Concepts:**
- Two hearts connected
- Interlinked circles or rings
- Stylized couple silhouette
- Abstract connection symbol
- Letter "T" with heart motif

### Creating the Icon

**Option 1: Online Tools (Easiest)**
1. Create a PNG image (256x256 or 512x512)
2. Use online converter:
   - https://icoconvert.com/
   - https://convertio.co/png-ico/
   - https://favicon.io/
3. Upload your PNG and download the .ico file
4. Place in `Together/Assets/together.ico`

**Option 2: Design Software**
1. Design in Adobe Illustrator, Figma, or Photoshop
2. Export at multiple sizes (16, 32, 48, 256)
3. Use icon editor to combine into .ico:
   - IcoFX (Windows)
   - Greenfish Icon Editor Pro (Free)
   - GIMP with ICO plugin

**Option 3: Use Existing Icon (Temporary)**
1. Find a suitable icon from:
   - https://icons8.com/
   - https://www.flaticon.com/
   - https://iconarchive.com/
2. Download as .ico format
3. Place in `Together/Assets/together.ico`

### After Creating the Icon

1. Place the icon file at `Together/Assets/together.ico`
2. Open `Together/Together.csproj`
3. Find line ~49 (commented out):
   ```xml
   <!-- <ApplicationIcon>Assets\together.ico</ApplicationIcon> -->
   ```
4. Uncomment it:
   ```xml
   <ApplicationIcon>Assets\together.ico</ApplicationIcon>
   ```
5. Rebuild the solution:
   ```powershell
   dotnet build Together.sln --configuration Release
   ```

### Verification

After adding the icon:
1. Build should complete without errors
2. Icon should appear on the executable
3. Icon should appear in Windows Explorer
4. Icon should appear on desktop shortcut after installation

### Placeholder

**Current Status:** ⚠️ Icon file not yet created

Until a custom icon is created:
- The application will use the default .NET application icon
- Build will succeed (icon reference is commented out)
- All functionality will work normally
- Only the visual branding will be missing

## Other Assets

This folder can also contain:
- Splash screen images
- Default profile pictures
- Application branding resources
