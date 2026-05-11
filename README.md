# Hardware

Production-grade .NET 10 Web API. Clean Architecture, JWT + refresh-token auth, EF Core/Postgres, Redis, Hangfire, SignalR, Serilog, OpenTelemetry via Aspire.


## First-run

```powershell
# 1. Restore tools + packages
dotnet tool restore
dotnet restore

# 2. Set up dev secrets (file is gitignored)
copy Hardware.API\appsettings.Development.template.json Hardware.API\appsettings.Development.json
# then edit Hardware.API\appsettings.Development.json — fill in real DB/Redis/JWT values

# 3. Bring up infra
docker compose up -d postgres redis

# 4. Generate the initial migration (entities are stable)
dotnet ef migrations add Initial `
  --project Hardware.Infrastructure `
  --startup-project Hardware.API `
  --output-dir Data/Migrations

# 5. Run
dotnet run --project Hardware.API
#    OR via Aspire dashboard
dotnet run --project Hardware.AppHost
```

Default URLs:
- Swagger: `http://localhost:<port>/swagger`
- Health: `http://localhost:<port>/api/v1/health`
- Hangfire: `http://localhost:<port>/hangfire` (Admin role)
- SignalR: `ws://localhost:<port>/hubs/notifications`

## What's wired up

| Area | Notes |
|------|-------|
| Auth | JWT bearer + refresh-token rotation + family revocation + Redis session revocation + per-user session cap + failed-login lockout |
| Persistence | EF Core 10 + Npgsql with `AddDbContextPool`, audit + slow-query interceptors, soft-delete via `IsDeleted` global filter |
| Cache | Redis (`ICacheService`) with memory fallback; OutputCache (`default`/`short`/`long`) with Redis backplane |
| Background jobs | Hangfire on Redis storage, queues `critical`/`default`/`notifications`/`reports`, Admin-gated dashboard at `/hangfire` |
| Real-time | SignalR (MessagePack + Redis backplane), JWT via `?access_token=` |
| Logging | Serilog (Console + rolling File + PostgreSQL sink) |
| Observability | OpenTelemetry traces/metrics/logs via `ServiceDefaults` |
| Errors | Global `ExceptionHandlingMiddleware` → RFC 7807 problem+json |
| Validation | FluentValidation via global `ValidationFilter` (auto-validation disabled) |
| API docs | Swagger UI with JWT Bearer security scheme |

## Hard rules

1. All `services.Add*` go in `Hardware.API/Startup/DependencyInjection.cs`.
2. Throw typed exceptions from `Hardware.Application/Exceptions` for HTTP errors — never `return BadRequest(...)`.
3. Validation = `AbstractValidator<TDto>` anywhere in `Hardware.Application` — auto-registered.
4. Settings = sealed records under `Hardware.Shared/Configuration` + `RegisterOptions`.
5. Package versions live in `Directory.Packages.props`.
6. Soft-delete: derive entities from `BaseEntity`/`AuditableEntity` and add `HasQueryFilter(e => !e.IsDeleted)`.
7. Don't bypass `ISessionRevocationStore` — it's load-bearing for stateless-JWT logout.
8. `appsettings.Development.json` is gitignored. Use the `.template` copy.

See `CLAUDE.md` for full architecture + conventions.
