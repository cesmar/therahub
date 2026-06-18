# TheraHub

Multi-tenant platform for independent psychologists to manage patients, scheduling, and clinical records.

## About the project

TheraHub is a SaaS platform designed for **independent psychologists** who run their own private practice without a central clinic. Each psychologist operates autonomously within the platform: their own patients, their own schedule, and their own billing — fully isolated from other psychologists using the same system.

This is a multi-tenant application, where each psychologist is treated as a tenant. All tenants share the same application instance and database, with data isolation enforced at the row level.

## Core modules (MVP)

1. **User management & authentication** — self-service onboarding for each psychologist (no clinic administrator required).
2. **Patient management** — patients belong exclusively to the psychologist who registered them.
3. **Scheduling & appointments**.
4. **Clinical records** — session notes and psychological history per patient.
5. **Session billing & payments** — internal record-keeping (no tax authority integration).
6. **Reports & dashboard** — personal insights per psychologist (not consolidated across tenants).
7. **General settings** — configurable per psychologist.

### Planned (post-MVP)

- **Platform billing** — subscription management for psychologists paying to use the platform (distinct from the billing psychologists issue to their own patients).

## Tech stack

### Backend

- **.NET** — Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Api`)
- **CQRS** with a custom mediator pattern implementation
- **FluentValidation** for input validation
- **Entity Framework Core** as the ORM
- **SQL Server** as the primary database

### Frontend

- **Angular 17**

### Infrastructure

- **Docker** (planned) — containerized backend API, frontend, and database for local development and future deployment

## Architecture

- **Style:** Modular monolith, single codebase and single database
- **Multi-tenancy:** Row-level isolation via a `TenantId` / `PsychologistId` discriminator on relevant tables
- **Application layer organization:** Vertical slices by feature/module within the Clean Architecture layers

## Repository structure

```
therahub/
├── .github/
│   └── workflows/          # CI/CD pipelines
├── backend/
│   ├── src/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Api/
│   └── tests/
├── frontend/
│   └── (Angular project)
├── docs/                   # Architecture notes and decisions
├── .gitignore
└── README.md
```

## Branching strategy

This repository follows a simplified GitHub Flow:

- `main` — protected branch, always deployable
- `feature/*` — new functionality (e.g. `feature/auth-onboarding`)
- `fix/*` — bug fixes
- `docs/*` — documentation-only changes

All changes are merged into `main` via Pull Requests.

## Status

🚧 Project in early development (MVP in progress).
