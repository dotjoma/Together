# Connection Request Migration

## Overview
This migration adds support for the couple connection request system.

## Database Changes

### New Table: connection_requests

```sql
CREATE TABLE connection_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    from_user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    to_user_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    responded_at TIMESTAMP,
    status VARCHAR(20) NOT NULL,
    CONSTRAINT valid_status CHECK (status IN ('Pending', 'Accepted', 'Rejected')),
    CONSTRAINT different_users CHECK (from_user_id != to_user_id)
);

CREATE INDEX idx_connection_requests_from_user ON connection_requests(from_user_id);
CREATE INDEX idx_connection_requests_to_user ON connection_requests(to_user_id);
CREATE INDEX idx_connection_requests_status ON connection_requests(status);
CREATE INDEX idx_connection_requests_users ON connection_requests(from_user_id, to_user_id);
```

## Entity Framework Core Migration

To create this migration, run:

```bash
dotnet ef migrations add AddConnectionRequest --project Together.Infrastructure --startup-project Together
```

To apply the migration:

```bash
dotnet ef database update --project Together.Infrastructure --startup-project Together
```

## Rollback

To rollback this migration:

```bash
dotnet ef database update PreviousMigrationName --project Together.Infrastructure --startup-project Together
```

To remove the migration:

```bash
dotnet ef migrations remove --project Together.Infrastructure --startup-project Together
```

## Notes
- The `from_user_id` uses CASCADE delete to automatically remove requests when a user is deleted
- The `to_user_id` uses RESTRICT delete to prevent deletion of users with pending requests
- Indexes are added for common query patterns (searching by user, filtering by status)
- The composite index on (from_user_id, to_user_id) optimizes duplicate request checks
