# Together.Infrastructure

This project contains infrastructure concerns including data access and external services.

## Contents

- **Data**: DbContext and Entity Framework configurations
- **Repositories**: Repository implementations
- **Services**: External service integrations (Supabase, SignalR)
- **SignalR**: Real-time communication hubs

## Dependencies

- Together.Domain (for entities and interfaces)
- Together.Application (for service interfaces)
- Entity Framework Core
- Npgsql (PostgreSQL provider)
- BCrypt.Net (password hashing)
- Supabase client libraries

## Rules

- Implements repository interfaces from Domain
- Contains all database-specific code
- Handles external service integrations
