# Backend rules — .NET 10 Web API + Clean Architecture

Stack: **.NET 10**, **EF Core 10 + Npgsql (PostgreSQL)**, **AutoMapper**,
**FluentValidation**, **Serilog**, **ClosedXML**. Clean Architecture with 4
projects.

## Layer boundaries

```
Domain          ←   Application   ←   Infrastructure
                                ←   API
```

- **Domain** has zero project references and zero NuGet dependencies. Pure
  C# entities + value objects + domain exceptions.
- **Application** references Domain only. Holds DTOs, service interfaces,
  service implementations, repository interfaces, AutoMapper profiles,
  FluentValidation rules, the `Result<T>` pattern.
- **Infrastructure** references Application and Domain. EF Core
  `DbContext`, Fluent API configurations, repository implementations,
  `UnitOfWork`, audit `SaveChangesInterceptor`, ClosedXML readers.
- **API** references Application and Infrastructure. Controllers,
  middleware, `Program.cs`, dependency wiring.

**Never invert these dependencies.** Domain does not know about EF Core.
Application does not know about ASP.NET Core. Controllers do not know about
DbContext.

## Controllers

- **Thin.** Route → call service → translate `Result<T>` to `IActionResult`.
  Zero business logic, zero EF queries.
- **Versioned routes**: `[Route("api/v1/<resource>")]`.
- **`[ApiController]`** always. Returns Problem+JSON for errors automatically.
- **Use `[FromBody]`, `[FromQuery]`, `[FromForm]` explicitly** when binding
  could be ambiguous (especially `[FromForm]` for `IFormFile` endpoints).
- **`CancellationToken` is the last parameter** of every async action.
- **HTTP status codes via `ResultExtensions.ToActionResult()`** — keeps the
  mapping centralized.

## Services and use cases

- **One interface per service** (`IStoreService`), one implementation
  (`StoreService`). Registered in `AddApplication()`.
- **Methods return `Result<T>`** (`NogoYa.Application.Common.Result`) for
  expected business failures (not found, validation, business rule violations).
  Throw only for truly exceptional cases (DB unavailable, programmer error).
- **Transactions for multi-step writes**: open via `IUnitOfWork.BeginTransactionAsync`,
  rollback on any failure, commit only at the end. See `OrderService.CreateAsync`.
- **`CancellationToken ct = default`** on every async signature.
- **Page size cap = 25** in every paginated endpoint. Enforce server-side; do
  not trust the client.

## Domain entities

- **All entities inherit `BaseEntity`** (Id, CreatedAt, UpdatedAt, IsDeleted,
  audit fields). `Guid` PKs across the board.
- **Soft-delete only.** Never `DELETE` rows from code; the
  `AuditSaveChangesInterceptor` translates `Remove()` to a flag flip + delete
  timestamp.
- **Business invariants in the entity**, not the service. Example: `Product.DecreaseStock(int qty)`
  validates `qty > 0` and `qty <= Stock` and throws `BusinessRuleException`.
- **Domain exceptions are the only thing the entity throws.** Infrastructure
  exceptions bubble up unaltered to the global handler.

## DTOs

- **`record` types** (positional with init properties). Immutable.
- **One file per aggregate root** (`StoreDtos.cs`, `ProductDtos.cs`), groups
  `XxxDto`, `CreateXxxDto`, `UpdateXxxDto`, `XxxFilterDto`.
- **Never expose entities to the API.** Map via AutoMapper in `MappingProfile`.
- **Filters carry pagination + search**: `int Page = 1, int PageSize = 25`.

## EF Core

- **Fluent API only**, in `Infrastructure/Persistence/Configurations/`. No
  data annotations on entities.
- **Snake_case table names** (`stores`, `products`, `order_items`,
  `audit_logs`). PascalCase column names (`"Name"`, `"CreatedAt"`).
- **Indexes on FKs and frequent-search columns.** Composite unique on
  `(StoreId, Sku)` filtered by `Sku IS NOT NULL`.
- **Check constraints for invariants** that should hold at the DB level
  (`Price >= 0`, `DiscountPercent BETWEEN 0 AND 100`, etc.).
- **Migrations auto-applied on startup** via `Program.cs`. Always test the
  migration locally with `dotnet ef database update`.
- **`jsonb` for audit payloads** (`OldValues`, `NewValues`). `decimal` columns
  always declare precision (`.HasPrecision(18, 2)`).
- **Concurrency**: `xmin` is the current strategy (PostgreSQL system column).
  If you turn it off temporarily, leave a `TODO`.

## Validation

- **FluentValidation in Application**, one validator per request DTO.
- Registered automatically via `AddValidatorsFromAssembly`. Errors translated
  to HTTP 400 by the global exception middleware.
- **DB-level guardrails (check constraints) are belt-and-suspenders.** Both
  the app and the DB enforce the rule.

## Logging and observability

- **Serilog with console sink.** Configuration in `appsettings.json`.
- **Structured logs**: `_logger.LogInformation("Order {OrderId} created", id)`,
  not `$"Order {id} created"`.
- **Every service logs major events** (create, transition, delete) at
  `Information`; errors at `Error` with the exception attached.
- **Never log PII or secrets** (passwords, tokens, connection strings).

## Auditing

- **`AuditSaveChangesInterceptor` runs automatically** on every save.
- Special detection: `PriceChange`, `StockChange` (Product) and `StatusChange`
  (Order). Other modifications are `Update`.
- **The interceptor never throws** — failure to audit must not break the
  request. Wrap in try/catch and log.

## Config and secrets

- **`appsettings.Local.json` is gitignored.** Local DB connection string lives
  there. The `.example` file at the same path is the template.
- **`DATABASE_URL`** env var is parsed at startup (`DatabaseUrlParser`) for
  Render/Heroku/Neon style URLs.
- **CORS allowed origins** come from `Cors:AllowedOrigins` config. Comma-
  separated for multiple domains. Never `AllowAnyOrigin()` in production code.

## NuGet feeds

- **`NuGet.Config` at `backend/` pins nuget.org only.** `<clear />` discards
  any corporate feed inherited from user-level config.
- **`dotnet-ef` is a local tool** in `backend/.config/dotnet-tools.json`.
  Restore with `dotnet tool restore`.

## What to avoid

- Returning entities from controllers.
- `Result.Failure("hardcoded message")` without an `ErrorCode` (the code is
  needed by `ResultExtensions` to choose the right HTTP status).
- Sync calls in async code (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`).
- `try { ... } catch { /* swallow */ }`. Either handle, rethrow, or wrap.
- New columns without a migration committed alongside.
- Magic numbers (`pageSize is > 100`). Define a `const int MaxPageSize = 25`.

## When in doubt

- Mirror the closest existing pattern (`StoreService` for new aggregate
  services, `ProductsController` for new resource controllers).
- Ask the user before adding a new NuGet package.
