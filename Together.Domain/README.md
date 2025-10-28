# Together.Domain

This project contains the domain layer of the Together application following Clean Architecture principles.

## Contents

- **Entities**: Core business entities (User, CoupleConnection, Post, MoodEntry, etc.)
- **ValueObjects**: Immutable value objects (Email, etc.)
- **Events**: Domain events
- **Interfaces**: Repository interfaces and domain service interfaces

## Dependencies

This layer has NO dependencies on other layers. It is the core of the application.

## Rules

- No dependencies on external frameworks
- Pure business logic only
- Entity Framework attributes should NOT be used here
