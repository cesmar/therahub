# TheraHub

Multi-tenant platform for psychology practices to manage client systems, clinical records, session notes and, later, scheduling and billing.

## About the project

TheraHub is a SaaS platform for **psychology practices**. A practice (`Practice`) is the tenant: it can have one or more psychologists, and its data is isolated from every other practice using the same system.

The clinical model follows **systemic family therapy**: the clinical "patient" is a **client system** (individual, couple or family), and a person (`Patient`) can belong to several client systems.

The pilot is a single practice with one psychologist. The first goal is to move paper clinical records into the system (patients seen since 2025), with clinical history, consent forms, scanned attachments and session notes.

## MVP scope

Planned delivery with test data: **Monday, December 21, 2026** (GitHub milestone `MVP demo – 21/12`).

| Block | Scope | Status |
|---|---|---|
| A | Database (DB v2 schema, test data, rule tests) | Done |
| B | Solution foundation: .NET 10 (#33), alignment with DB v2 (#34), Angular 22 setup (#22, #23, #24, #26) | In progress |
| C | Patients & client systems | Pending |
| D | Clinical records and informed consent | Pending |
| E | Attachments (scans, genograms) | Pending |
| F | Session notes (without addenda) | Pending |
| G | Login, off-machine backups, HTTPS (#21, #25) — after the demo | Pending |

### Later (post-MVP)

- Scheduling and appointments (#13), rates and payments
- Financial reports and dashboard
- Addenda to session notes, changing the psychologist from the UI, access log screen
- Practice invitations and platform billing (subscriptions)

## Tech stack

### Backend

- **.NET 10 LTS** (upgrade from .NET 9 in #33) — Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Api`) with controllers
- **CQRS** with a custom mediator (no MediatR) and `Result<T>` for expected failures
- **FluentValidation** for input validation
- **Entity Framework Core**, **DB-first**: entities and configurations are mapped by hand to the existing tables (no EF migrations; removed in #34)
- **Dapper** allowed for read-only reports
- **SQL Server** only

### Database

- The schema is **managed outside this repository**: T-SQL scripts run with **DbUp** (schemas `Sec`, `Prac`, `Cnfg`, `Clin`, `Sched`, `Bill`, `Audit`, `Doc`)
- Local database: `db_dev_therahub`
- Business rules that must never be broken are enforced in the database (triggers with `THROW 50001–50010`); the API translates those errors into user messages

### Frontend

- **Angular 22** (standalone, zoneless) with strict TypeScript
- **Tailwind** with the TheraHub Design System tokens (Lora for headings, Nunito Sans for UI)
- Dev proxy to the API (single origin in development, no CORS)

### Infrastructure

- Local demos on the home network during the MVP
- CI on pull requests (#3) and Docker (#4–#6): backlog

## Architecture

- **Style:** modular monolith, single codebase and single database
- **Multi-tenancy:** row-level isolation by `PracticeId`, with EF Core named query filters (`Tenant` and `SoftDelete`). Tables without `PracticeId` (clinical record, consent, emergency contact, system members) are always read through their `ClientSystem`
- **Psychologist scope:** applied in the handlers (not as a global filter), so practice-wide features can still read across psychologists
- **Audit and soft delete:** set by a `SaveChanges` interceptor (`CreatedBy` / `UpdatedBy` = user name)
- **Errors:** handlers return `Result<T>`; the API maps them to `ProblemDetails` (validation errors by field, camelCase keys)

## Repository structure

```
therahub/
├── .github/
│   └── workflows/              # CI (planned, #3)
├── backend/
│   ├── CLAUDE.md               # Context for Claude Code
│   ├── TheraHub.sln
│   ├── src/
│   │   ├── TheraHub.Domain/
│   │   ├── TheraHub.Application/
│   │   ├── TheraHub.Infrastructure/
│   │   └── TheraHub.Api/
│   └── tests/                  # (no test projects yet)
├── frontend/                   # Angular 22 app (#22)
├── docs/                       # Architecture notes and decisions
├── .gitignore
└── README.md
```

## Running locally

### Backend

Requirements: .NET SDK (10 once #33 is merged), SQL Server with the `db_dev_therahub` database created by the DbUp scripts.

```bash
cd backend
dotnet build
dotnet run --project src/TheraHub.Api   # http://localhost:5084
```

- Health check: `GET http://localhost:5084/api/health`
- Connection string (`ConnectionStrings:DefaultConnection`): use `dotnet user-secrets` in local development (set up in #34). Never commit credentials; `appsettings.Development.json` is git-ignored.

### Frontend

Instructions will be added with #22 and #23.

## Branching strategy

This repository follows a simplified GitHub Flow:

- `main` — protected branch; changes only through pull requests
- `feature/<issue>-<short-description>` — new functionality (e.g. `feature/33-dotnet-10`)
- `fix/*` — bug fixes
- `docs/*` — documentation-only changes

Pull requests close their issue with `Closes #N`. Commits follow conventional commits (`feat:`, `fix:`, `docs:`, `refactor:`, `chore:`, `test:`).

## Status

🚧 MVP in progress — block B (solution foundation).
