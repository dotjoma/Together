# Together Application - Clean Architecture Project Structure

## Overview

The Together application follows Clean Architecture principles with clear separation of concerns across four main projects.

## Project Structure

```
Together/
├── Together.Domain/                    # Core domain layer (no dependencies)
│   ├── Entities/                       # Domain entities
│   ├── ValueObjects/                   # Immutable value objects
│   ├── Events/                         # Domain events
│   └── Interfaces/                     # Repository and service interfaces
│
├── Together.Application/               # Application/Business logic layer
│   ├── Services/                       # Application services
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Interfaces/                    # Service interfaces
│   └── Commands/                      # Command objects
│
├── Together.Infrastructure/            # Infrastructure layer
│   ├── Data/                          # DbContext and EF configurations
│   ├── Repositories/                  # Repository implementations
│   ├── Services/                      # External service integrations
│   └── SignalR/                       # Real-time communication
│
└── Together/ (Presentation)            # WPF Presentation layer
    ├── Views/                         # XAML views
    ├── ViewModels/                    # MVVM ViewModels
    ├── Converters/                    # Value converters
    ├── Behaviors/                     # Attached behaviors
    └── Controls/                      # Custom controls
```

## Dependency Flow

```
Presentation → Application → Domain ← Infrastructure
```

- **Presentation** depends on Application and Infrastructure (for DI setup)
- **Application** depends on Domain
- **Infrastructure** depends on Domain and Application
- **Domain** has NO dependencies (pure business logic)

## Installed NuGet Packages

### Together (Presentation)
- MaterialDesignThemes 5.3.0
- Microsoft.Extensions.DependencyInjection 9.0.10

### Together.Infrastructure
- Microsoft.EntityFrameworkCore 9.0.10
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
- BCrypt.Net-Next 4.0.3
- supabase-csharp 0.16.2

### Together.Application
- (References Domain only)

### Together.Domain
- (No external dependencies)

## Dependency Injection Setup

The DI container is configured in `Together/App.xaml.cs`:
- Services are registered in the `ConfigureServices` method
- The container is built on application startup
- Services are resolved throughout the application lifecycle

## Build Status

✅ All projects build successfully
✅ Project references configured correctly
✅ NuGet packages restored
✅ Clean Architecture principles enforced

## Next Steps

1. Create domain entities in Together.Domain
2. Define repository interfaces in Together.Domain
3. Implement application services in Together.Application
4. Set up DbContext and repositories in Together.Infrastructure
5. Create ViewModels and Views in Together (Presentation)
