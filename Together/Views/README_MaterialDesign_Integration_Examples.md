# Material Design Integration Examples

## Overview

This document provides practical examples of integrating the new Material Design theming components into existing views.

## Example 1: Adding Loading States to SocialFeedView

### Before
```xml
<Grid>
    <ListBox ItemsSource="{Binding Posts}"/>
</Grid>
```

### After
```xml
<Grid>
    <!-- Main Content -->
    <ListBox ItemsSource="{Binding Posts}"
             Visibility="{Binding HasPosts, Converter={StaticResource BooleanToVisibilityConverter}}"/>
    
    <!-- Empty State -->
    <controls:EmptyStateView IconKind="Post"
                             Title="No Posts Yet"
                             Message="Follow users to see their posts in your feed!"
                             ActionButtonText="Find Users"
                             ActionCommand="{Binding FindUsersCommand}"
                             Visibility="{Binding HasPosts, Converter={StaticResource InverseBooleanToVisibilityConverter}}"/>
    
    <!-- Loading Overlay -->
    <controls:LoadingOverlay IsLoading="{Binding IsLoading}" 
                             LoadingMessage="Loading posts..."/>
</Grid>
```

### ViewModel Changes
```csharp
private bool _isLoading;
public bool IsLoading
{
    get => _isLoading;
    set => SetProperty(ref _isLoading, value);
}

public bool HasPosts => Posts?.Any() == true;

private async Task LoadPostsAsync()
{
    IsLoading = true;
    try
    {
        var posts = await _socialFeedService.GetFeedAsync(userId, 0, 20);
        Posts = new ObservableCollection<PostDto>(posts);
        OnPropertyChanged(nameof(HasPosts));
    }
    finally
    {
        IsLoading = false;
    }
}
```

## Example 2: Adding Empty State to JournalView

### XAML
```xml
<Grid>
    <!-- Journal Entries List -->
    <ScrollViewer Visibility="{Binding HasEntries, Converter={StaticResource BooleanToVisibilityConverter}}">
        <ItemsControl ItemsSource="{Binding JournalEntries}">
            <!-- Entry template -->
        </ItemsControl>
    </ScrollViewer>
    
    <!-- Empty State -->
    <controls:EmptyStateView IconKind="Book"
                             Title="No Journal Entries"
                             Message="Start documenting your journey together by creating your first journal entry!"
                             ActionButtonText="Write Entry"
                             ActionCommand="{Binding CreateEntryCommand}"
                             Visibility="{Binding HasEntries, Converter={StaticResource InverseBooleanToVisibilityConverter}}"/>
    
    <!-- Loading -->
    <controls:LoadingOverlay IsLoading="{Binding IsLoading}"/>
</Grid>
```

## Example 3: Applying Typography Styles

### Before
```xml
<StackPanel>
    <TextBlock Text="Mood Tracker" FontSize="24" FontWeight="Medium"/>
    <TextBlock Text="How are you feeling today?" FontSize="14"/>
    <TextBlock Text="Last updated: 2 hours ago" FontSize="12" Opacity="0.6"/>
</StackPanel>
```

### After
```xml
<StackPanel>
    <TextBlock Text="Mood Tracker" Style="{StaticResource TitleTextStyle}"/>
    <TextBlock Text="How are you feeling today?" Style="{StaticResource BodyTextStyle}"/>
    <TextBlock Text="Last updated: 2 hours ago" Style="{StaticResource CaptionTextStyle}"/>
</StackPanel>
```

## Example 4: Using Card Styles

### Before
```xml
<Border Background="White" 
        Padding="16" 
        Margin="8"
        CornerRadius="4">
    <!-- Content -->
</Border>
```

### After
```xml
<materialDesign:Card Style="{StaticResource CardStyle}">
    <!-- Content -->
</materialDesign:Card>
```

### Interactive Card
```xml
<materialDesign:Card Style="{StaticResource InteractiveCardStyle}"
                     MouseDown="Card_MouseDown">
    <!-- Content -->
</materialDesign:Card>
```

## Example 5: Adding Floating Action Button

### XAML
```xml
<Grid>
    <!-- Main content -->
    <ScrollViewer>
        <!-- Content -->
    </ScrollViewer>
    
    <!-- FAB positioned at bottom-right -->
    <Button Style="{StaticResource FabStyle}"
            Command="{Binding CreateCommand}"
            HorizontalAlignment="Right"
            VerticalAlignment="Bottom"
            Margin="0,0,24,24"
            ToolTip="Create New Post">
        <materialDesign:PackIcon Kind="Plus" Width="24" Height="24"/>
    </Button>
</Grid>
```

## Example 6: Adding Section Headers and Dividers

### Before
```xml
<StackPanel>
    <TextBlock Text="Recent Activity" FontWeight="Bold"/>
    <!-- Content -->
    <Rectangle Height="1" Fill="Gray" Margin="0,8"/>
    <TextBlock Text="Upcoming Events" FontWeight="Bold"/>
    <!-- Content -->
</StackPanel>
```

### After
```xml
<StackPanel>
    <TextBlock Text="Recent Activity" Style="{StaticResource SectionHeaderStyle}"/>
    <!-- Content -->
    <Separator Style="{StaticResource DividerStyle}"/>
    <TextBlock Text="Upcoming Events" Style="{StaticResource SectionHeaderStyle}"/>
    <!-- Content -->
</StackPanel>
```

## Example 7: Adding List Item Animations

### Before
```xml
<ListBox ItemsSource="{Binding Items}">
    <ListBox.ItemTemplate>
        <!-- Template -->
    </ListBox.ItemTemplate>
</ListBox>
```

### After
```xml
<ListBox ItemsSource="{Binding Items}"
         ItemContainerStyle="{StaticResource AnimatedListItemStyle}">
    <ListBox.ItemTemplate>
        <!-- Template -->
    </ListBox.ItemTemplate>
</ListBox>
```

## Example 8: Adding Chips for Tags

### XAML
```xml
<ItemsControl ItemsSource="{Binding Tags}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <materialDesign:Chip Style="{StaticResource CustomChipStyle}"
                                 Content="{Binding}"
                                 IsDeletable="True"
                                 DeleteCommand="{Binding DataContext.RemoveTagCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                                 DeleteCommandParameter="{Binding}"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Example 9: Adding Badge to Notification Icon

### XAML
```xml
<materialDesign:Badged Badge="{Binding UnreadCount}" 
                       Style="{StaticResource BadgeStyle}"
                       BadgeColorZoneMode="SecondaryMid">
    <Button Style="{StaticResource MaterialDesignIconButton}"
            Command="{Binding ShowNotificationsCommand}">
        <materialDesign:PackIcon Kind="Bell" Width="24" Height="24"/>
    </Button>
</materialDesign:Badged>
```

## Example 10: Responsive Card Grid

### XAML
```xml
<ScrollViewer>
    <ItemsControl ItemsSource="{Binding Challenges}"
                  Style="{StaticResource ResponsiveCardGridStyle}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <materialDesign:Card Style="{StaticResource CardStyle}"
                                     Width="300"
                                     Margin="8">
                    <!-- Challenge content -->
                </materialDesign:Card>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

## Common Patterns

### Pattern 1: Content with Loading and Empty States

```xml
<Grid>
    <!-- Content (visible when has data and not loading) -->
    <ContentControl Content="{Binding Data}"
                    Visibility="{Binding HasData, Converter={StaticResource BooleanToVisibilityConverter}}"/>
    
    <!-- Empty State (visible when no data and not loading) -->
    <controls:EmptyStateView Visibility="{Binding ShowEmptyState, Converter={StaticResource BooleanToVisibilityConverter}}"
                             IconKind="..."
                             Title="..."
                             Message="..."/>
    
    <!-- Loading Overlay (visible when loading) -->
    <controls:LoadingOverlay IsLoading="{Binding IsLoading}"/>
</Grid>
```

### Pattern 2: Form with Validation

```xml
<StackPanel>
    <TextBlock Text="Create Post" Style="{StaticResource TitleTextStyle}"/>
    
    <TextBox materialDesign:HintAssist.Hint="What's on your mind?"
             Text="{Binding Content, UpdateSourceTrigger=PropertyChanged}"
             Style="{StaticResource MaterialDesignOutlinedTextBox}"
             MaxLength="500"
             TextWrapping="Wrap"
             AcceptsReturn="True"
             MinHeight="100"
             Margin="0,8"/>
    
    <TextBlock Text="{Binding CharacterCount}"
               Style="{StaticResource CaptionTextStyle}"
               HorizontalAlignment="Right"/>
    
    <Button Content="Post"
            Command="{Binding PostCommand}"
            Style="{StaticResource MaterialDesignRaisedButton}"
            HorizontalAlignment="Right"
            Margin="0,16,0,0"/>
</StackPanel>
```

### Pattern 3: Dashboard Widget

```xml
<materialDesign:Card Style="{StaticResource CardStyle}">
    <StackPanel>
        <TextBlock Text="Love Streak" Style="{StaticResource SubtitleTextStyle}"/>
        <Separator Style="{StaticResource DividerStyle}"/>
        
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,16">
            <materialDesign:PackIcon Kind="Fire" 
                                     Width="48" 
                                     Height="48"
                                     Foreground="{StaticResource SecondaryBrush}"/>
            <TextBlock Text="{Binding StreakCount}"
                       Style="{StaticResource HeadlineTextStyle}"
                       VerticalAlignment="Center"
                       Margin="8,0,0,0"/>
        </StackPanel>
        
        <TextBlock Text="days in a row"
                   Style="{StaticResource CaptionTextStyle}"
                   HorizontalAlignment="Center"/>
    </StackPanel>
</materialDesign:Card>
```

## Migration Checklist

For each view, ensure:

- [ ] Replace custom TextBlock styles with predefined typography styles
- [ ] Replace Border elements with materialDesign:Card where appropriate
- [ ] Add LoadingOverlay for async operations
- [ ] Add EmptyStateView for empty collections
- [ ] Apply AnimatedListItemStyle to list items
- [ ] Use SectionHeaderStyle for section titles
- [ ] Use DividerStyle for separators
- [ ] Add FAB for primary actions where appropriate
- [ ] Apply responsive grid styles for card layouts
- [ ] Test at different window sizes (minimum 1024x600)

## Performance Tips

1. **Lazy Loading**: Only load visible items in virtualized lists
2. **Image Caching**: Use ImageCacheService for profile pictures
3. **Debouncing**: Use DebouncedAction for search inputs
4. **Async Operations**: Always show loading indicators for operations > 100ms
5. **Animation Performance**: Keep animations under 300ms for responsiveness

## Accessibility Considerations

1. **Keyboard Navigation**: Ensure all interactive elements are keyboard accessible
2. **Screen Readers**: Use AutomationProperties.Name for icons and buttons
3. **Color Contrast**: Verify text meets WCAG AA standards (4.5:1 for normal text)
4. **Focus Indicators**: Material Design provides built-in focus indicators
5. **Loading Feedback**: Always provide visual feedback for async operations

## Testing

Test each updated view for:

1. **Loading States**: Verify loading overlay appears during async operations
2. **Empty States**: Verify empty state appears when no data
3. **Animations**: Verify smooth transitions (no jank)
4. **Responsiveness**: Test at minimum window size (1024x600)
5. **Typography**: Verify consistent font sizes and weights
6. **Colors**: Verify theme colors applied correctly
7. **Hover Effects**: Verify interactive elements respond to hover
8. **Keyboard**: Verify tab navigation works correctly
