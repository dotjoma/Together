# Together.Application

This project contains the application layer with business logic and use cases.

## Contents

- **Services**: Application services implementing business logic
- **DTOs**: Data Transfer Objects for communication between layers
- **Interfaces**: Service interfaces
- **Commands**: Command objects for CQRS pattern (if needed)

## Dependencies

- Together.Domain (for entities and domain interfaces)

## Rules

- Contains business logic and orchestration
- No UI concerns
- No database implementation details
