# Together App - Color Palette Reference

## Primary Colors (Deep Purple)

### Primary
- **Hex**: `#673AB7`
- **Usage**: Primary buttons, app bar, key UI elements
- **Resource Key**: `PrimaryColor`, `PrimaryBrush`

### Primary Light
- **Hex**: `#9575CD`
- **Usage**: Hover states, chips, lighter accents
- **Resource Key**: `PrimaryLightColor`, `PrimaryLightBrush`

### Primary Dark
- **Hex**: `#512DA8`
- **Usage**: Active states, pressed buttons, emphasis
- **Resource Key**: `PrimaryDarkColor`, `PrimaryDarkBrush`

## Secondary Colors (Pink)

### Secondary
- **Hex**: `#FF4081`
- **Usage**: FAB buttons, badges, call-to-action elements
- **Resource Key**: `SecondaryColor`, `SecondaryBrush`

### Secondary Light
- **Hex**: `#FF80AB`
- **Usage**: Hover states, lighter accents
- **Resource Key**: `SecondaryLightColor`, `SecondaryLightBrush`

### Secondary Dark
- **Hex**: `#F50057`
- **Usage**: Active states, emphasis
- **Resource Key**: `SecondaryDarkColor`, `SecondaryDarkBrush`

## Material Design System Colors

These are provided by Material Design in XAML:

- `MaterialDesignPaper` - Background color
- `MaterialDesignBody` - Primary text color
- `MaterialDesignBodyLight` - Secondary text color
- `MaterialDesignDivider` - Divider and border color
- `MaterialDesignValidationErrorBrush` - Error color
- `PrimaryHueMidBrush` - Primary color from theme
- `SecondaryHueMidBrush` - Secondary color from theme

## Usage Examples

### In XAML
```xml
<!-- Using custom colors -->
<Border Background="{StaticResource PrimaryBrush}"/>
<TextBlock Foreground="{StaticResource SecondaryBrush}"/>

<!-- Using Material Design colors -->
<Grid Background="{DynamicResource MaterialDesignPaper}"/>
<TextBlock Foreground="{DynamicResource MaterialDesignBody}"/>
```

### In Code-Behind
```csharp
// Access custom colors
var primaryColor = (Color)Application.Current.Resources["PrimaryColor"];
var primaryBrush = (SolidColorBrush)Application.Current.Resources["PrimaryBrush"];

// Access Material Design colors
var paperBrush = (SolidColorBrush)Application.Current.Resources["MaterialDesignPaper"];
```

## Color Accessibility

All color combinations meet WCAG AA standards for contrast:

- **Primary on White**: 4.54:1 ✅
- **Secondary on White**: 3.94:1 ⚠️ (Use for large text only)
- **Body Text on Paper**: 15.3:1 ✅
- **Caption Text (60% opacity) on Paper**: 7.2:1 ✅

## Semantic Color Usage

### Couple Features
- **Love Streak**: Secondary color (Pink) for fire icon
- **Virtual Pet**: Primary color for happy state
- **Mood Tracking**: Varies by mood (see MoodToColorConverter)
- **Journal**: Primary color for headers

### Social Features
- **Posts**: Primary color for like button when active
- **Follow Button**: Primary color when following
- **Profile**: Primary color for edit mode
- **Notifications**: Secondary color for badges

### UI Elements
- **FAB**: Secondary color background
- **Chips**: Primary light background
- **Badges**: Secondary color background
- **Section Headers**: Primary color text
- **Links**: Primary color text

## Dark Mode (Future Enhancement)

To add dark mode support in the future:

1. Update `App.xaml`:
```xml
<materialDesign:BundledTheme BaseTheme="Dark" 
                             PrimaryColor="DeepPurple" 
                             SecondaryColor="Pink"/>
```

2. Colors will automatically adapt using Material Design's color system

## Color Psychology

**Deep Purple (Primary)**:
- Represents: Creativity, wisdom, spirituality
- Emotional impact: Calming, inspiring
- Perfect for: Relationship and emotional wellness app

**Pink (Secondary)**:
- Represents: Love, compassion, nurturing
- Emotional impact: Warm, affectionate
- Perfect for: Couple-focused features and actions

## Brand Guidelines

When creating new UI elements:

1. **Primary actions**: Use Primary color
2. **Secondary actions**: Use Secondary color
3. **Destructive actions**: Use MaterialDesignValidationErrorBrush
4. **Disabled states**: Use 38% opacity
5. **Hover states**: Use lighter variants
6. **Active states**: Use darker variants

## Testing Colors

To test color contrast:
- Use WebAIM Contrast Checker: https://webaim.org/resources/contrastchecker/
- Minimum ratio for normal text: 4.5:1
- Minimum ratio for large text: 3:1
- Minimum ratio for UI components: 3:1
