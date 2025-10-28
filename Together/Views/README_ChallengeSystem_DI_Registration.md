# Challenge System - Dependency Injection Registration

## Required DI Registrations

Add the following registrations to your `App.xaml.cs` in the `ConfigureServices` method:

```csharp
// Challenge Repository
services.AddScoped<IChallengeRepository, ChallengeRepository>();

// Challenge Service
services.AddScoped<IChallengeService, ChallengeService>();

// Challenge ViewModel (if using DI for ViewModels)
services.AddTransient<ChallengeViewModel>();
```

## Complete Example

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // ... existing registrations ...

    // Challenge System
    services.AddScoped<IChallengeRepository, ChallengeRepository>();
    services.AddScoped<IChallengeService, ChallengeService>();
    services.AddTransient<ChallengeViewModel>();
}
```

## Usage in Views

### Option 1: Constructor Injection (Recommended)
```csharp
public partial class ChallengeView : UserControl
{
    public ChallengeView(ChallengeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

### Option 2: Manual Creation
```csharp
var challengeService = serviceProvider.GetRequiredService<IChallengeService>();
var currentUserId = GetCurrentUserId(); // Your method to get current user
var connectionId = GetConnectionId(); // Your method to get connection

var viewModel = new ChallengeViewModel(challengeService, currentUserId, connectionId);
var view = new ChallengeView { DataContext = viewModel };
```

## Navigation Integration

If using a navigation service, register the view:

```csharp
navigationService.RegisterView<ChallengeViewModel, ChallengeView>();
```

Then navigate:

```csharp
navigationService.NavigateTo<ChallengeViewModel>();
```

## Background Service (Optional)

For automatic daily challenge generation and expired challenge cleanup:

```csharp
public class ChallengeBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    public ChallengeBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1);
            var delay = midnight - now;
            
            await Task.Delay(delay, stoppingToken);
            
            using var scope = _serviceProvider.CreateScope();
            var challengeService = scope.ServiceProvider.GetRequiredService<IChallengeService>();
            var connectionRepository = scope.ServiceProvider.GetRequiredService<ICoupleConnectionRepository>();
            
            // Generate challenges for all active connections
            var connections = await connectionRepository.GetAllActiveConnectionsAsync();
            foreach (var connection in connections)
            {
                await challengeService.GenerateDailyChallengeAsync(connection.Id);
            }
            
            // Archive expired challenges
            await challengeService.ArchiveExpiredChallengesAsync();
        }
    }
}

// Register the background service
services.AddHostedService<ChallengeBackgroundService>();
```

## Dependencies Required

The Challenge system depends on:
- `IChallengeRepository` → `ChallengeRepository`
- `ICoupleConnectionRepository` → `CoupleConnectionRepository`
- `IUserRepository` → `UserRepository`

Ensure these are registered before the Challenge service.
