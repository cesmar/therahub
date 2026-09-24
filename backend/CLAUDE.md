# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TheraHub** is a multi-tenant SaaS platform for independent psychologists to manage their own patients, scheduling, and clinical records. Each psychologist is an isolated tenant — they never see each other's data.

- **Solo developer:** Carlos (senior .NET + Angular fullstack freelancer)
- **Pilot client:** One psychologist financing initial development
- **Goal:** Scale to multiple paying psychologists after MVP

---

## Commands

```bash
# Build the solution
dotnet build

# Run the API (HTTP on localhost:5084)
dotnet run --project src/TheraHub.Api/TheraHub.Api.csproj

# Run with HTTPS profile (localhost:7133)
dotnet run --project src/TheraHub.Api/TheraHub.Api.csproj --launch-profile https

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/<TestProjectName>/<TestProjectName>.csproj

# Restore packages
dotnet restore

# Format code
dotnet format
```

---

## Architecture

The project follows **Clean Architecture** with four layers. Dependencies flow inward: `Api → Infrastructure → Application → Domain`.

```
src/
  TheraHub.Domain/         # Entities, value objects, domain rules
  TheraHub.Application/    # Use cases, abstractions, validation pipeline
  TheraHub.Infrastructure/ # Mediator implementation, EF Core, infrastructure services
  TheraHub.Api/            # ASP.NET Core 9 Web API, controllers, DI wiring
tests/                     # Test projects (no test projects created yet — directory is a placeholder)
```

### Key architectural decisions

- **No MediatR** — avoided due to commercial licensing from v13. The custom mediator is intentional and must be preserved.
- **Modular monolith** — not microservices. Do not suggest splitting into separate services.
- **Vertical slices by feature** inside the Application layer — each feature/module has its own folder.
- **EF Core without engine-specific SQL** — keeps the door open for future migration from SQL Server to PostgreSQL. Never use raw SQL or SQL Server-specific functions.

---

## Multi-Tenancy (Critical)

Multi-tenancy is **row-level isolation** using a `PracticeId` discriminator on every tenant-scoped entity. This is a core invariant — never omit it.

- **`Practice`** (`TheraHub.Domain/Entities/Practice.cs`) is the tenant entity — referred to as "Tenant" in planning/issues, but the actual class name is `Practice`. Each Practice maps to one Psychologist.
- **Hybrid onboarding:** Carlos (admin) activates the Practice (`IsActive` / `ActivatedAt`); the psychologist completes their own profile.
- **`ICurrentTenant`** (`TheraHub.Application/Abstractions/Tenancy/ICurrentTenant.cs`) exposes `PracticeId`. It's implemented by `Infrastructure/Services/CurrentTenant.cs`, which reads the `practiceId` claim off the current `HttpContext.User` — it throws if there's no authenticated user, so it can't be resolved outside a request (e.g. in background jobs) without a different implementation.
- **Global query filters are applied per-entity, not automatically.** `TheraHubDbContext.OnModelCreating` calls `ApplyConfigurationsFromAssembly` for conventional config, then explicitly applies each tenant-scoped `IEntityTypeConfiguration<T>` (see `UserConfiguration`, `PsychologistConfiguration`). Each of those configs calls `builder.HasQueryFilter(e => e.PracticeId == _dbContext.CurrentPracticeId && !e.IsDeleted)` in `Configure()`. **When adding a new tenant-scoped entity, you must wire this up by hand**: give the configuration class a constructor taking `TheraHubDbContext`, add the query filter, and add the `new XConfiguration(this)` line in `OnModelCreating`. There is no base-class or reflection mechanism doing this for you.
- **Tenant-scoped configuration classes must depend on `TheraHubDbContext`, not `ICurrentTenant`, in their query filters.** `TheraHubDbContext` exposes `public int CurrentPracticeId => _currentTenant.PracticeId;` specifically for this. EF Core caches the built model across requests, so a filter closure that captures an injected `ICurrentTenant` instance directly gets permanently bound to whichever DbContext instance happened to build the model first (a real bug we hit and fixed). Filters rooted at a member access on the DbContext type itself are rebound by EF Core to the actual current instance at query time — that's why the property must live on the DbContext, and configuration classes must hold a reference to the DbContext (passed as `this` from `OnModelCreating`) rather than to `ICurrentTenant` or any other captured service.
- **Never expose cross-tenant data** — all queries must be scoped by PracticeId (either via the query filter above or explicitly for non-tenant-scoped lookups).

---

## Stack

| Concern | Technology |
|---|---|
| Backend framework | ASP.NET Core 9 |
| ORM | EF Core (SQL Server — no engine-specific SQL) |
| Database (local) | SQL Server Developer |
| Database (production pilot) | SQL Server Express |
| Validation | FluentValidation |
| Frontend | Angular 22 |
| API testing | Postman |
| IDE | Visual Studio + VS Code |

---

## Custom Mediator / CQRS Pattern

The project implements its **own CQRS mediator** (not MediatR). Understanding this pattern is essential for adding new features.

**Abstractions** (defined in `TheraHub.Application/Abstractions/Mediator/`):
- `ICommand<TResult>` / `IQuery<TResult>` — marker interfaces for requests
- `ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>` — handler contracts
- `ISender` — dispatches commands and queries through the pipeline via `SendCommandAsync<TResult>(command, ct)` and `SendQueryAsync<TResult>(query, ct)` (there is no single `Send()` method — commands and queries go through separate entry points)
- `IPipelineBehavior<TRequest, TResult>` — middleware interface for cross-cutting concerns
- `Result<T>` — return type with `.IsSuccess`, `.Value`, `.Error` (no `.IsFailure` property — check `!result.IsSuccess`)

**Adding a new use case:**
1. Create a record implementing `ICommand<Result<T>>` or `IQuery<Result<T>>` in `Application`
2. Add a `IValidator<YourCommand>` (FluentValidation) if validation is needed
3. Implement `ICommandHandler<YourCommand, Result<T>>` or `IQueryHandler<...>`
4. Handlers and validators auto-register via reflection scanning in each layer's `DependencyInjection.cs` (`AddApplication` scans for `ICommandHandler<,>`/`IQueryHandler<,>` implementations and validators) — no manual registration needed
5. Inject `ISender` into a controller and call `sender.SendCommandAsync(command, cancellationToken)` or `sender.SendQueryAsync(...)`

**Pipeline execution order:**
`ISender.SendCommandAsync()`/`SendQueryAsync()` → `ValidationBehavior` (FluentValidation) → handler → `Result<T>`

**See** `Application/Test/TestCommand.cs` and `Api/Controllers/HealthController.cs` for working examples of the full flow.

> ⚠️ C# does not infer generic type arguments from return types. Always specify `<TResult>` explicitly when calling `ExecutePipelineAsync<TResult>(...)`.

---

## Key Conventions

- **Target Framework**: .NET 9.0 across all projects
- **Nullable reference types** and **implicit usings** are enabled globally
- All handlers are async; return `Task<Result<T>>` rather than throwing exceptions
- `Result<T>` is the standard return type for all handler responses — do not throw for expected failures
- FluentValidation validators live alongside their command/query in the `Application` layer
- Never use Singleton services that depend on Scoped services (captive dependency anti-pattern)

---

## MVP Modules

| # | Module | Status |
|---|---|---|
| 1 | Auth & Users | 🔄 In progress |
| 2 | Patients | 📋 Pending |
| 3 | Scheduling & Appointments | 📋 Pending |
| 4 | Clinical Records | 📋 Pending |
| 5 | Billing & Payments (internal only — no tax authority integration) | 📋 Pending |
| 6 | Reports & Dashboard (per psychologist, not consolidated) | 📋 Pending |
| 7 | General Settings per psychologist | 📋 Pending |

> **Out of MVP scope:** Platform billing module (subscription charged to psychologists for using TheraHub).

---

## GitHub Issues — Backend Execution Order

| Issue | Description | Status |
|---|---|---|
| #14 | Create .NET solution and project structure | ✅ Done |
| #15 | Implement custom mediator pattern | ✅ Done |
| #18 | Define core domain entities and base abstractions | ✅ Done |
| #19 | Configure EF Core with multi-tenant global query filters | 🔄 In progress |
| #20 | Implement Tenant activation flow (hybrid onboarding) | 📋 Pending |
| #21 | Implement Auth: registration + login + JWT | 📋 Pending |

---

## Git Workflow
 
- **Never commit directly to `main`** — it is a protected branch.
- **Always create a feature branch first:**
```bash
  git checkout -b feature/<issue-number>-<short-description>
  # or
  git checkout -b fix/<short-description>
  git checkout -b docs/<short-description>
```
- **Commit on the feature branch**, then push:
```bash
  git push origin <branch-name>
```
- **Never open PRs or push to main directly** — remind the user to open the PR manually on GitHub.
- **Always include `Closes #N`** in commit messages or PR body to auto-close the linked Issue.
- **Prefer small, frequent commits** with descriptive messages using conventional commits format:
  - `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`

---

## What NOT to do

- ❌ Do not use MediatR — the custom mediator is intentional
- ❌ Do not use engine-specific SQL (no `GETDATE()`, no SQL Server-only functions)
- ❌ Do not omit PracticeId from any new tenant-scoped entity, or forget to wire its `HasQueryFilter` in `TheraHubDbContext.OnModelCreating` (it is not automatic)
- ❌ Do not expose data across tenants in any query
- ❌ Do not throw exceptions for expected business failures — use `Result<T>`
- ❌ Do not register handlers or validators manually — they auto-register via reflection