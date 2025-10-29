# Virtual Pet System - Dependency Injection Registration

## Service Registration in App.xaml.cs

The following services have been registered in the `ConfigureServices` method:

```csharp
// In the Repositories section
services.AddScoped<IVirtualPetRepository, VirtualPetRepository>();

// In the Application Services section
services.AddScoped<IVirtualPetService, VirtualPetService>();
```

## Converter Registration in App.xaml

The following converters have been registered in the Application Resources:

```xml
<converters:PetStateToColorConverter x:Key="PetStateToColorConverter"/>
<converters:AppearanceToColorConverter x:Key="AppearanceToColorConverter"/>
<converters:StringEqualityConverter x:Key="StringEqualityConverter"/>
```

## Usage in ViewModels

### VirtualPetViewModel
```csharp
public VirtualPetViewModel(IVirtualPetService petService, Guid connectionId)
{
    _petService = petService;
    _connectionId = connectionId;
    // ...
}
```

### PetCustomizationViewModel
```csharp
public PetCustomizationViewModel(IVirtualPetService petService, Guid petId)
{
    _petService = petService;
    _petId = petId;
    // ...
}
```

### VirtualPetWidgetViewModel
```csharp
public VirtualPetWidgetViewModel(IVirtualPetService petService, Guid connectionId)
{
    _petService = petService;
    _connectionId = connectionId;
    // ...
}
```

## Integration with Other Services

Other services that need to award XP should inject `IVirtualPetService`:

### Example: JournalService
```csharp
public class JournalService : IJournalService
{
    private readonly IJournalEntryRepository _journalRepository;
    private readonly IVirtualPetService _virtualPetService;
    
    public JournalService(
        IJournalEntryRepository journalRepository,
        IVirtualPetService virtualPetService)
    {
        _journalRepository = journalRepository;
        _virtualPetService = virtualPetService;
    }
    
    public async Task<JournalEntryDto> CreateEntryAsync(CreateJournalEntryDto dto)
    {
        // Create journal entry
        var entry = new JournalEntry(/* ... */);
        await _journalRepository.AddAsync(entry);
        
        // Award XP to virtual pet
        await _virtualPetService.AddExperienceAsync(
            entry.ConnectionId, 
            InteractionType.JournalEntry);
        
        return MapToDto(entry);
    }
}
```

### Example: MoodTrackingService
```csharp
public class MoodTrackingService : IMoodTrackingService
{
    private readonly IMoodEntryRepository _moodRepository;
    private readonly IVirtualPetService _virtualPetService;
    private readonly ICoupleConnectionRepository _connectionRepository;
    
    public MoodTrackingService(
        IMoodEntryRepository moodRepository,
        IVirtualPetService virtualPetService,
        ICoupleConnectionRepository connectionRepository)
    {
        _moodRepository = moodRepository;
        _virtualPetService = virtualPetService;
        _connectionRepository = connectionRepository;
    }
    
    public async Task<MoodEntryDto> CreateMoodEntryAsync(CreateMoodEntryDto dto)
    {
        // Create mood entry
        var entry = new MoodEntry(/* ... */);
        await _moodRepository.AddAsync(entry);
        
        // Get user's connection
        var connection = await _connectionRepository.GetByUserIdAsync(dto.UserId);
        if (connection != null)
        {
            // Award XP to virtual pet
            await _virtualPetService.AddExperienceAsync(
                connection.Id, 
                InteractionType.MoodLog);
        }
        
        return MapToDto(entry);
    }
}
```

### Example: ChallengeService
```csharp
public async Task<bool> CompleteChallengeAsync(Guid challengeId, Guid userId)
{
    var challenge = await _challengeRepository.GetByIdAsync(challengeId);
    // ... mark as complete ...
    
    // Award XP when both partners complete
    if (challenge.CompletedByUser1 && challenge.CompletedByUser2)
    {
        await _virtualPetService.AddExperienceAsync(
            challenge.ConnectionId, 
            InteractionType.ChallengeCompletion);
    }
    
    return true;
}
```

### Example: TodoService
```csharp
public async Task<TodoItemDto> MarkAsCompleteAsync(Guid todoId)
{
    var todo = await _todoRepository.GetByIdAsync(todoId);
    todo.MarkAsComplete();
    await _todoRepository.UpdateAsync(todo);
    
    // Award XP to virtual pet
    await _virtualPetService.AddExperienceAsync(
        todo.ConnectionId, 
        InteractionType.TodoCompletion);
    
    return MapToDto(todo);
}
```

## Dashboard Integration

To add the virtual pet widget to the Couple Hub dashboard:

```csharp
public class CoupleHubViewModel : ViewModelBase
{
    private readonly IVirtualPetService _virtualPetService;
    private VirtualPetWidgetViewModel? _virtualPetWidgetViewModel;
    
    public CoupleHubViewModel(
        // ... other services ...
        IVirtualPetService virtualPetService)
    {
        _virtualPetService = virtualPetService;
        // ...
    }
    
    public VirtualPetWidgetViewModel? VirtualPetWidgetViewModel
    {
        get => _virtualPetWidgetViewModel;
        set => SetProperty(ref _virtualPetWidgetViewModel, value);
    }
    
    private async Task LoadDashboardAsync()
    {
        // ... load other data ...
        
        // Initialize virtual pet widget
        if (_currentConnection != null)
        {
            VirtualPetWidgetViewModel = new VirtualPetWidgetViewModel(
                _virtualPetService, 
                _currentConnection.Id);
        }
    }
}
```

And in the XAML:
```xml
<controls:VirtualPetWidget 
    DataContext="{Binding VirtualPetWidgetViewModel}"
    Visibility="{Binding VirtualPetWidgetViewModel, Converter={StaticResource NullToVisibilityConverter}}"/>
```

## Automatic Pet Creation

To automatically create a pet when a couple connection is established:

```csharp
public class CoupleConnectionService : ICoupleConnectionService
{
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IVirtualPetService _virtualPetService;
    
    public CoupleConnectionService(
        ICoupleConnectionRepository connectionRepository,
        IVirtualPetService virtualPetService)
    {
        _connectionRepository = connectionRepository;
        _virtualPetService = virtualPetService;
    }
    
    public async Task<CoupleConnection> AcceptConnectionRequestAsync(Guid requestId)
    {
        // ... create connection ...
        var connection = new CoupleConnection(/* ... */);
        await _connectionRepository.AddAsync(connection);
        
        // Create virtual pet for the new connection
        await _virtualPetService.CreatePetAsync(connection.Id, "Our Pet");
        
        return connection;
    }
}
```

## Complete Registration Checklist

✅ IVirtualPetRepository registered in App.xaml.cs
✅ VirtualPetRepository registered in App.xaml.cs
✅ IVirtualPetService registered in App.xaml.cs
✅ VirtualPetService registered in App.xaml.cs
✅ PetStateToColorConverter registered in App.xaml
✅ AppearanceToColorConverter registered in App.xaml
✅ StringEqualityConverter registered in App.xaml

## Next Integration Steps

1. ✅ Services registered
2. ✅ Converters registered
3. ⏳ Inject IVirtualPetService into CoupleConnectionService
4. ⏳ Add pet creation in AcceptConnectionRequestAsync
5. ⏳ Inject IVirtualPetService into JournalService
6. ⏳ Add XP calls in CreateEntryAsync
7. ⏳ Inject IVirtualPetService into MoodTrackingService
8. ⏳ Add XP calls in CreateMoodEntryAsync
9. ⏳ Inject IVirtualPetService into ChallengeService
10. ⏳ Add XP calls in CompleteChallengeAsync
11. ⏳ Inject IVirtualPetService into TodoService
12. ⏳ Add XP calls in MarkAsCompleteAsync
13. ⏳ Add VirtualPetWidgetViewModel to CoupleHubViewModel
14. ⏳ Add VirtualPetWidget to CoupleHub view
15. ⏳ Add navigation to VirtualPetView
16. ⏳ Add navigation to PetCustomizationView
