# Together.Presentation

This is the WPF presentation layer of the Together application.

## Contents

- **Views**: XAML views
- **ViewModels**: ViewModels following MVVM pattern
- **Converters**: Value converters for data binding
- **Behaviors**: Attached behaviors
- **Controls**: Custom controls

## Dependencies

- Together.Application (for services)
- Together.Infrastructure (for DI registration)
- MaterialDesignThemes (UI framework)
- Microsoft.Extensions.DependencyInjection

## Rules

- MVVM pattern strictly followed
- No business logic in code-behind
- ViewModels should not reference Views
- All services injected via constructor
