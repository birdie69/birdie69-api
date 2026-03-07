# birdie69-api

> .NET 8 ASP.NET Core REST API for **birdie69** — a daily-question relationship app for couples.

**Status:** Scaffold pending (Day 2)

---

## Overview

Clean Architecture + DDD + CQRS backend API.

| Layer | Project | Description |
|-------|---------|-------------|
| Domain | `Birdie69.Domain` | Entities, Value Objects, Domain Events, Interfaces |
| Application | `Birdie69.Application` | Use Cases (CQRS via MediatR), DTOs, Validation |
| Infrastructure | `Birdie69.Infrastructure` | EF Core, Redis, Azure SDK, External clients |
| Presentation | `Birdie69.Api` | ASP.NET Core Controllers, Middleware, DI |

## Architecture

See [birdie69-docs/ARCHITECTURE_OVERVIEW.md](https://github.com/learn-claude/birdie69-docs/blob/main/ARCHITECTURE_OVERVIEW.md)

## ADRs

- [ADR-003: Backend .NET 8](https://github.com/learn-claude/birdie69-docs/blob/main/adrs/ADR-003-backend-dotnet8.md)
- [ADR-002: Auth Azure AD B2C](https://github.com/learn-claude/birdie69-docs/blob/main/adrs/ADR-002-auth-azure-ad-b2c.md)

## Prerequisites

- .NET 8 SDK
- Docker Desktop (for PostgreSQL + Redis)
- Azure AD B2C tenant (or local mock for dev)

## Development

```bash
# Start dependencies
docker compose up -d

# Run API
dotnet run --project src/Birdie69.Api

# Run tests
dotnet test
```

## Jira

[B69 Project](https://narwhal.atlassian.net/projects/B69) — Tickets: B69-2, B69-9+
