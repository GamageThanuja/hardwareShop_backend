# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

.NET 10 (SDK pinned via `global.json` to `10.0.201`, `rollForward: latestFeature`). C# `latest`, nullable + implicit usings, file-scoped namespaces enforced. ASP.NET Core MVC controllers (NOT Minimal APIs). EF Core 10 + Npgsql. StackExchange.Redis. Hangfire on Redis. SignalR (MessagePack + Redis backplane). JWT bearer auth. Serilog (Console + File + PostgreSQL sink). Swashbuckle. .NET Aspire orchestrator + ServiceDefaults. Central Package Management via `Directory.Packages.props` — package versions go there, not in individual `.csproj` files.

`TreatWarningsAsErrors=true` in Release. `EnforceCodeStyleInBuild=true`. EditorConfig requires file-scoped namespaces (warning-level diagnostic).

Solution file is `Hardware.slnx` (XML format, not legacy `.sln`).

## Common commands

```bash
# Restore + build (Release fails on warnings)
dotnet tool restore           # required once — installs dotnet-ef from dotnet-tools.json
dotnet restore
dotnet build -c Release

# Run API directly
dotnet run --project Hardware.API

# Run via Aspire (orchestrates Postgres + Redis + API)
dotnet run --project Hardware.AppHost

# Bring up infra without Aspire
docker compose up -d postgres redis

# EF Core migrations — must specify both --project (Infrastructure) and --startup-project (API).
# Migrations live in Hardware.Infrastructure/Data/Migrations.
dotnet ef migrations add <Name> \
  --project Hardware.Infrastructure \
  --startup-project Hardware.API \
  --output-dir Data/Migrations

dotnet ef database update \
  --project Hardware.Infrastructure \
  --startup-project Hardware.API
```

In Development, `MigrationRunner.ApplyMigrationsAsync` runs `Database.MigrateAsync()` + seeds the admin user on startup (gated by `SeedData:EnableSeeding`). Don't call `EnsureCreated`. **In Production migrations are NOT auto-applied — run `dotnet ef database update` as a deploy step.**

An `Initial` migration already exists (`Hardware.Infrastructure/Data/Migrations/20260510072600_Initial`). Only run `ef migrations add` when adding new entities or changing existing ones.

There are currently **no test projects** in the solution. Don't invent `dotnet test` instructions until tests exist.

## Configuration & secrets

- `appsettings.json` = production-shaped values with empty/placeholder secrets. Production overrides via env vars (`Jwt__SecurityKey`, `ConnectionStrings__DefaultConnection`, etc.) or Key Vault.
- `appsettings.Development.json` = source of truth for dev-time JWT key, Postgres/Redis connection strings, SMTP. **Gitignored.** Bootstrap by copying `appsettings.Development.template.json` → `appsettings.Development.json`.
- `dotnet user-secrets` is **not** used — keep dev secrets in `appsettings.Development.json`.
- All settings bind through the Options pattern with `ValidateDataAnnotations` + `ValidateOnStart`. Config records live in `Hardware.Shared/Configuration` as sealed records — add new settings there, then bind in `DependencyInjection.RegisterOptions`.

## Architecture (Clean Architecture, 7 projects)

```
Domain          # Entities, BaseEntity/AuditableEntity, repository/UoW/CurrentUser interfaces, enums
Shared          # Sealed-record settings (Options pattern), constants (RoleConstants, CustomClaimTypes), helpers
Application     # Service contracts, DTOs, AutoMapper profile, FluentValidation validators,
                # ApiResponse<T>, PagedResult, typed exceptions (NotFound/Unauthorized/Forbidden/Conflict/Business/Validation)
Infrastructure  # ApplicationDbContext + Audit/SlowQuery interceptors, repositories,
                # Identity/JWT generation, Redis cache, Hangfire jobs, EF Migrations
API             # Controllers (versioned under v1/), middlewares, SignalR hubs, Program.cs, DI orchestrator
ServiceDefaults # Aspire shared: OTel tracing/metrics/logs, health checks, service discovery, http resilience
AppHost         # Aspire DistributedApplication entry point
```

Reference direction is one-way inward: API → Application → Domain; Infrastructure → Application/Domain. Domain depends on nothing.

### DI orchestrator pattern (load-bearing)

**All `services.Add*` calls live in `Hardware.API/Startup/DependencyInjection.cs`**, organized as small `RegisterX` extension methods called from `RegisterServices`. Controllers/middlewares must NOT call `services.Add*` directly. When wiring a new dependency:

1. Define interface in `Domain/Interfaces` (or `Application` for service contracts).
2. Implement in `Infrastructure` (or `Application` for pure orchestration).
3. Register inside the right `RegisterX` method in `DependencyInjection.cs`.
4. If it's a settings record, also add it to `RegisterOptions`.

Note: `AuthService` lives in `Infrastructure/Identity` (needs `UserManager` + `DbContext`) but is registered in `RegisterApplicationServices` to keep the conceptual layering clear. Don't move it.

### Request pipeline (`Program.cs`)

Order matters and is tuned — preserve it when adding middleware:

```
SerilogRequestLogging → CorrelationIdMiddleware → ExceptionHandlingMiddleware
→ Swagger (dev or AppSettings:EnableSwagger) → ResponseCompression → Routing
→ CORS → RateLimiter → OutputCache
→ [hangfire access_token → cookie shim]
→ Authentication → Authorization
→ HangfireDashboard("/hangfire") → MapDefaultEndpoints (Aspire) → MapControllers → MapHub<NotificationHub>("/hubs/notifications")
```

Kestrel is tuned at startup (1000 concurrent connections, 25 MB body limit, ThreadPool min/max set). Don't undo without reason.

### Error handling

`ExceptionHandlingMiddleware` is the single source of HTTP error responses — emits RFC 7807 `application/problem+json`. **Throw typed exceptions** from `Application.Exceptions` (`NotFoundException`, `UnauthorizedException`, `ForbiddenException`, `ConflictException`, `BusinessException`, `ValidationException`); do not write `return BadRequest(...)` from controllers for error states. The middleware maps each type to a status code and includes the `correlationId` from `HttpContext.TraceIdentifier`.

### Validation

FluentValidation auto-validation is **disabled** (`DisableDataAnnotationsValidation = true`, `SuppressModelStateInvalidFilter = true`). Validation runs through the global `ValidationFilter` (`API/Filters/ValidationFilter.cs`) which reflectively resolves `IValidator<T>` for each action argument and throws `ValidationException` on failure. To validate a new DTO: add an `AbstractValidator<TDto>` anywhere in `Hardware.Application` — it's auto-registered via `AddValidatorsFromAssemblyContaining<AutoMapperProfile>`.

### Persistence

- `ApplicationDbContext` registered with `AddDbContextPool` + Npgsql retry-on-failure (3, 10s), 30s command timeout, `SplitQuery` default, batch size 4–42.
- `AuditInterceptor` + `SlowQueryInterceptor` registered as **singletons** (DbContextPool's factory uses the root `IServiceProvider` — scoped services can't be resolved there). Both are stateless; per-request data is read via singleton `IHttpContextAccessor`. Do not change them to scoped without reworking pool registration.
- Entities derive from `BaseEntity` or `AuditableEntity` (`Domain/Common`). Soft delete is enforced via the `IsDeleted` global query filter and the audit interceptor — `Remove(...)` becomes an `UPDATE`.
- `IGenericRepository<T>` + `IUnitOfWork` are the standard data-access seams (registered open-generic).

### Auth

JWT bearer with role-based policies in `RegisterAuthorization`. Out of the box only `RequireAdmin` is wired — add domain-specific policies (e.g. `RequireDoctor`, `RequireOperator`) as you define new roles. Role names live in `Shared/Constants/RoleConstants`. Custom claims (e.g. `SessionId`) in `CustomClaimTypes`.

`OnTokenValidated` checks `ISessionRevocationStore` (Redis) by `SessionId` — revoked sessions fail authentication. `OnMessageReceived` accepts `?access_token=` for `/hubs` and `/hangfire` (Hangfire also reads from the `hw_hangfire_jwt` cookie set by the small inline middleware in `Program.cs`). The Hangfire dashboard is gated by `HangfireAuthorizationFilter` which requires the `Admin` role.

Identity password policy: 8+ chars, digit + lower + upper + non-alphanumeric, lockout 5 attempts / 15 min.

#### Refresh tokens & sessions

- Refresh token: 256-bit base64-url opaque string. Stored server-side as **SHA-256 hash** in `RefreshTokens`. Sliding 14d / absolute 90d (defaults).
- Returned both in JSON body and in cookie `rt` — `HttpOnly` + `Secure` + `SameSite=Strict`, scoped to `/api/v1/auth`.
- Rotation links rows via `FamilyId` + `ReplacedByTokenId`. Reusing an already-rotated token revokes the entire family and returns error code `TOKEN_REUSE`.
- `MaxActiveSessionsPerUser` (default 10): on login, oldest sessions beyond cap are revoked.
- Failed-login lockout: Redis counter `auth:failed_login:{email}` with 30 min TTL, max `MaxFailedLoginAttempts` (default 5). Returns `ACCOUNT_LOCKED`; counter cleared on success.
- Logout-all bulk-revokes all active tokens + adds each `SessionId` to the Redis revocation store with TTL = `AccessTokenExpirationMinutes + 1`.

### Controllers

Versioned under `Controllers/v1/`. Derive from `AppControllerBase` (`API/Common/AppControllerBase.cs`) — its primary constructor takes `ILogger` (not `ILogger<T>`), so inject it and pass it up. Return `ApiResponse<T>` from `Application/Common`. Use `GetUserId/GetUserEmail/GetUserName/IsAdmin` helpers and `StartActivity/SetActivityTag` for OpenTelemetry spans.

### Caching

`ICacheService` (Redis) for app-level distributed cache with memory fallback. `OutputCache` policies: `default` (60s), `short` (30s), `long` (10m) — backed by Redis when configured. Apply via `[OutputCache(PolicyName = "long")]` on actions. SignalR uses Redis backplane with channel prefix `Hardware:signalr`.

### Rate limiting

Three named policies registered in `RegisterRateLimiting`: `default` (per-IP, fixed window), `auth_endpoints` (tighter limit for auth routes), `admin` (per-username/IP for admin routes). Apply via `[EnableRateLimiting("auth_endpoints")]` on a controller or action. Limits are configured in `RateLimitingSettings` (`appsettings`).

### Background jobs

Hangfire queues: `critical`, `default`, `notifications`, `reports`. Workers = `Environment.ProcessorCount * HangfireSettings.WorkerCountMultiplier` (default 5). Redis storage prefix `Hardware:hangfire:`, server name `Hardware-{MachineName}`. Recurring jobs are wired in `HangfireJobConfiguration.ConfigureRecurringJobs(app.Services)` — add new ones there.

### Observability

`builder.AddServiceDefaults()` (from `ServiceDefaults/Extensions.cs`) wires OpenTelemetry tracing/metrics/logs, health checks (`/health`, `/alive` — dev only via `MapDefaultEndpoints`), service discovery, and HTTP resilience defaults. `/api/v1/health` is the public anonymous health probe. Slow-query threshold lives in `AppSettings:SlowQueryThresholdMs` (default 1000 ms) and is used by `SlowQueryInterceptor`.

## Adding a new aggregate (checklist)

The domain currently only has the Identity aggregate. For each new aggregate:

1. **Entity** — `Domain/Entities/<Aggregate>/<Entity>.cs`, extending `BaseEntity` or `AuditableEntity`.
2. **EF config** — `Infrastructure/Data/Configurations/<Entity>Configuration.cs` implementing `IEntityTypeConfiguration<T>`. `ApplyConfigurationsFromAssembly` picks it up automatically.
3. **DbSet** — Add `public DbSet<Entity> Entities => Set<Entity>();` to `ApplicationDbContext`.
4. **Service contract** — `Application/Services/<Feature>/I<Feature>Service.cs`.
5. **DTOs** — `Application/DTOs/<Feature>/`.
6. **Validator** — `Application/Validators/<Feature>/<Dto>Validator.cs` extending `AbstractValidator<TDto>`. Auto-discovered from the `AutoMapperProfile` assembly.
7. **AutoMapper** — Add mapping in `Application/Mappings/AutoMapperProfile.cs`.
8. **Implementation** — In `Infrastructure/` if it needs `DbContext`/`UserManager`; in `Application/` for pure orchestration.
9. **Register** — Add to `RegisterApplicationServices` or `RegisterInfrastructureServices` in `DependencyInjection.cs`.
10. **Controller** — `API/Controllers/v1/<Feature>Controller.cs` extending `AppControllerBase`.
11. **Migration** — `dotnet ef migrations add <Name> --project Hardware.Infrastructure --startup-project Hardware.API --output-dir Data/Migrations`.

## Conventions

- File-scoped namespaces (warning if not).
- 4-space indent for `.cs`, 2-space for JSON/csproj/yaml/md.
- Sort `using` directives system-first, single group.
- Don't add package `Version=` attributes in `.csproj`; add the version in `Directory.Packages.props` and reference by name.
- Add new roles to `RoleConstants` (`Shared/Constants/RoleConstants.cs`) and include them in `RoleConstants.All` — the seeder creates every role in `All` on startup automatically.
- Follow `RefreshTokenConfiguration` (`Infrastructure/Data/Configurations/`) as the reference EF configuration pattern.
- **Don't bypass `ISessionRevocationStore`.** Server-side logout depends on the `OnTokenValidated` hook checking it on every request — don't add an "optimization" that skips the Redis lookup.
