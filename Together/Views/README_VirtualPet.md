# Virtual Pet System Implementation

## Overview
The Virtual Pet system provides a gamified engagement feature where couples can care for a shared virtual pet that evolves based on their interaction frequency.

## Components Created

### Domain Layer
- **IVirtualPetRepository** (`Together.Domain/Interfaces/IVirtualPetRepository.cs`)
  - Repository interface for virtual pet data access

### Infrastructure Layer
- **VirtualPetRepository** (`Together.Infrastructure/Repositories/VirtualPetRepository.cs`)
  - Implements CRUD operations for virtual pets
  - Includes connection relationship loading

### Application Layer
- **IVirtualPetService** (`Together.Application/Interfaces/IVirtualPetService.cs`)
  - Service interface defining pet operations
  
- **VirtualPetService** (`Together.Application/Services/VirtualPetService.cs`)
  - Implements pet creation, XP management, level-up logic
  - Manages pet state based on interaction frequency
  - Handles customization with unlock validation
  - XP values per interaction type:
    - Journal Entry: 20 XP
    - Mood Log: 10 XP
    - Chat Message: 5 XP
    - Challenge Completion: 30 XP
    - Todo Completion: 15 XP
  - Level unlocks:
    - Level 1: default, blue, pink
    - Level 5: green, yellow
    - Level 10: purple, orange
    - Level 15: rainbow, galaxy
    - Level 20: golden, diamond

- **VirtualPetDto** (`Together.Application/DTOs/VirtualPetDto.cs`)
  - Data transfer object for pet information

### Presentation Layer

#### ViewModels
- **VirtualPetViewModel** (`Together/ViewModels/VirtualPetViewModel.cs`)
  - Main view model for pet display
  - Handles pet state updates and refresh
  - Manages navigation to customization

- **PetCustomizationViewModel** (`Together/ViewModels/PetCustomizationViewModel.cs`)
  - Handles pet name and appearance customization
  - Validates unlocked appearances
  - Provides preview functionality

- **VirtualPetWidgetViewModel** (`Together/ViewModels/VirtualPetWidgetViewModel.cs`)
  - Compact view model for dashboard widget
  - Displays essential pet information

#### Views
- **VirtualPetView** (`Together/Views/VirtualPetView.xaml`)
  - Full pet display with animated visualization
  - Shows level, XP bar, and current state
  - Provides access to customization

- **PetCustomizationView** (`Together/Views/PetCustomizationView.xaml`)
  - Pet name editing
  - Appearance selection from unlocked options
  - Live preview of changes

#### Controls
- **VirtualPetWidget** (`Together/Controls/VirtualPetWidget.xaml`)
  - Compact widget for dashboard display
  - Shows pet avatar, level, state, and XP progress

#### Converters
- **PetStateToColorConverter** (`Together/Converters/PetStateToColorConverter.cs`)
  - Converts pet state and appearance to display color
  - Modifies color based on emotional state (happy, sad, neglected, excited)

- **AppearanceToColorConverter** (`Together/Converters/AppearanceToColorConverter.cs`)
  - Maps appearance names to colors

- **StringEqualityConverter** (`Together/Converters/StringEqualityConverter.cs`)
  - Used for radio button binding in appearance selection

## Features Implemented

### Pet Creation
- Automatically created when couple connection is established
- Starts at level 1 with default appearance
- Initial state is Happy

### Experience System
- Gains XP from couple interactions
- Different interaction types award different XP amounts
- Automatic level-up when XP threshold reached (Level * 100 XP)
- XP carries over to next level

### Level System
- Levels unlock new appearance options
- Pet becomes more customizable as level increases
- Visual feedback when leveling up (Excited state)

### State Management
- **Happy**: Regular interaction (< 1 day since last interaction)
- **Sad**: Infrequent interaction (1-3 days)
- **Neglected**: No interaction for 3+ days
- **Excited**: Just leveled up or special events

### Customization
- Name can be changed at any time
- Appearance options unlock at specific levels
- Validation prevents using locked appearances
- Live preview before saving

### Visual Design
- Simple, friendly pet design using circles and paths
- Color-coded states for easy recognition
- Smooth animations and transitions
- Material Design styling throughout

## Integration Points

### Service Registration
Services are registered in `App.xaml.cs`:
```csharp
services.AddScoped<IVirtualPetRepository, VirtualPetRepository>();
services.AddScoped<IVirtualPetService, VirtualPetService>();
```

### Converter Registration
Converters are registered in `App.xaml`:
```xml
<converters:PetStateToColorConverter x:Key="PetStateToColorConverter"/>
<converters:AppearanceToColorConverter x:Key="AppearanceToColorConverter"/>
<converters:StringEqualityConverter x:Key="StringEqualityConverter"/>
```

### Usage in Other Services
Other services should call `IVirtualPetService.AddExperienceAsync()` when interactions occur:
```csharp
// Example: After creating a journal entry
await _virtualPetService.AddExperienceAsync(connectionId, InteractionType.JournalEntry);
```

### Dashboard Integration
Add VirtualPetWidget to the Couple Hub dashboard:
```xml
<controls:VirtualPetWidget DataContext="{Binding VirtualPetWidgetViewModel}"/>
```

## Requirements Satisfied

✅ **Requirement 9.1**: Pet creation on couple connection establishment
✅ **Requirement 9.2**: Experience points from interactions
✅ **Requirement 9.3**: Level-up with appearance unlocks
✅ **Requirement 9.4**: State detection for 3-day inactivity
✅ **Requirement 9.5**: Customization for name and appearance

## Future Enhancements

1. **Animations**: Add more sophisticated animations for state changes
2. **Sound Effects**: Add audio feedback for level-ups and interactions
3. **Pet Types**: Allow choosing different pet types (cat, dog, dragon, etc.)
4. **Accessories**: Add unlockable accessories and decorations
5. **Pet Interactions**: Add mini-games or direct interaction features
6. **Pet History**: Track pet growth history and milestones
7. **Multiple Pets**: Allow couples to have multiple pets
8. **Pet Sharing**: Share pet achievements on social feed

## Testing Recommendations

1. **Unit Tests**:
   - Test XP calculation for different interaction types
   - Test level-up logic with various XP amounts
   - Test state updates based on last interaction date
   - Test appearance unlock validation

2. **Integration Tests**:
   - Test pet creation when couple connection is established
   - Test XP addition from actual interactions
   - Test customization with database persistence

3. **UI Tests**:
   - Verify pet visualization updates correctly
   - Test customization flow end-to-end
   - Verify widget displays correct information
