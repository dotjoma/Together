# Material Design Quick Start Guide

## 5-Minute Integration Guide

This guide shows you how to quickly add Material Design theming to your views.

## Step 1: Add Namespace (if not already present)

```xml
xmlns:controls="clr-namespace:Together.Presentation.Controls"
```

## Step 2: Add Loading State

### In ViewModel
```csharp
private bool _isLoading;
public bool IsLoading
{
    get => _isLoading;
    set => SetProperty(ref _isLoading, value);
}
```

### In XAML
```xml
<Grid>
    <!-- Your content here -->
    
    <controls:LoadingOverlay IsLoading="{Binding IsLoading}" 
                             LoadingMessage="Loading..."/>
</Grid>
```

## Step 3: Add Empty State

### In ViewModel
```csharp
public bool HasItems => Items?.Any() == true;
```

### In XAML
```xml
<controls:EmptyStateView IconKind="YourIcon"
                         Title="No Items"
                         Message="Your message here"
                         Visibility="{Binding HasItems, Converter={StaticResource InverseBooleanToVisibilityConverter}}"/>
```

## Step 4: Apply Typography

Replace this:
```xml
<TextBlock Text="Title" FontSize="24" FontWeight="Medium"/>
```

With this:
```xml
<TextBlock Text="Title" Style="{StaticResource TitleTextStyle}"/>
```

## Step 5: Use Cards

Replace this:
```xml
<Border Background="White" Padding="16" CornerRadius="4">
    <!-- Content -->
</Border>
```

With this:
```xml
<materialDesign:Card Style="{StaticResource CardStyle}">
    <!-- Content -->
</materialDesign:Card>
```

## Common Styles Reference

### Typography
- `HeadlineTextStyle` - 34px (Page titles)
- `TitleTextStyle` - 24px (Section titles)
- `SubtitleTextStyle` - 20px (Subsections)
- `BodyTextStyle` - 14px (Body text)
- `CaptionTextStyle` - 12px (Captions, timestamps)

### Components
- `CardStyle` - Standard card
- `InteractiveCardStyle` - Card with hover effect
- `FabStyle` - Floating action button
- `SectionHeaderStyle` - Section headers
- `DividerStyle` - Separators
- `CustomChipStyle` - Tags/chips
- `BadgeStyle` - Notification badges

### Animations
- `AnimatedListItemStyle` - For ListBox items
- `PageTransitionStyle` - For page content

## Common Icons

```xml
<materialDesign:PackIcon Kind="Post"/>        <!-- Posts -->
<materialDesign:PackIcon Kind="Book"/>        <!-- Journal -->
<materialDesign:PackIcon Kind="EmoticonHappy"/> <!-- Mood -->
<materialDesign:PackIcon Kind="CheckboxMarkedOutline"/> <!-- Todos -->
<materialDesign:PackIcon Kind="Calendar"/>    <!-- Events -->
<materialDesign:PackIcon Kind="Heart"/>       <!-- Love/Couple -->
<materialDesign:PackIcon Kind="AccountGroup"/> <!-- Social -->
<materialDesign:PackIcon Kind="Plus"/>        <!-- Add -->
<materialDesign:PackIcon Kind="Close"/>       <!-- Close -->
<materialDesign:PackIcon Kind="Check"/>       <!-- Confirm -->
```

## Complete Example

```xml
<UserControl xmlns:controls="clr-namespace:Together.Presentation.Controls">
    <Grid>
        <!-- Header -->
        <TextBlock Text="My View" 
                   Style="{StaticResource TitleTextStyle}"
                   Margin="16"/>
        
        <!-- Content -->
        <ScrollViewer Visibility="{Binding HasItems, Converter={StaticResource BooleanToVisibilityConverter}}">
            <ItemsControl ItemsSource="{Binding Items}"
                          ItemContainerStyle="{StaticResource AnimatedListItemStyle}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <materialDesign:Card Style="{StaticResource CardStyle}">
                            <TextBlock Text="{Binding Name}" 
                                       Style="{StaticResource BodyTextStyle}"/>
                        </materialDesign:Card>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
        
        <!-- Empty State -->
        <controls:EmptyStateView IconKind="InboxArrowDown"
                                 Title="No Items"
                                 Message="Get started by adding your first item!"
                                 ActionButtonText="Add Item"
                                 ActionCommand="{Binding AddCommand}"
                                 Visibility="{Binding HasItems, Converter={StaticResource InverseBooleanToVisibilityConverter}}"/>
        
        <!-- Loading -->
        <controls:LoadingOverlay IsLoading="{Binding IsLoading}"/>
        
        <!-- FAB -->
        <Button Style="{StaticResource FabStyle}"
                Command="{Binding AddCommand}"
                HorizontalAlignment="Right"
                VerticalAlignment="Bottom"
                Margin="24">
            <materialDesign:PackIcon Kind="Plus" Width="24" Height="24"/>
        </Button>
    </Grid>
</UserControl>
```

## Color Usage

```xml
<!-- Primary color -->
<Border Background="{StaticResource PrimaryBrush}"/>

<!-- Secondary color -->
<Border Background="{StaticResource SecondaryBrush}"/>

<!-- Material Design colors -->
<Grid Background="{DynamicResource MaterialDesignPaper}"/>
<TextBlock Foreground="{DynamicResource MaterialDesignBody}"/>
```

## Need More Help?

See detailed documentation:
- `README_MaterialDesign_Theming.md` - Complete documentation
- `README_MaterialDesign_Integration_Examples.md` - More examples
- `ColorPalette.md` - Color reference
