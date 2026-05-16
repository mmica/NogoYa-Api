# Nogo-Ya — Claude project guide

This file is the entry point for any Claude session working on this repo.
Read it first; it tells you what the project is, how it is laid out, and where
to find the conventions you must follow.

## What is Nogo-Ya

A local marketplace MVP that connects the merchants of **Nogoyá, Entre Ríos**
with local consumers. It is a monorepo:

```
backend/   .NET 10 Web API (Clean Architecture, 4 projects)
frontend/  Angular 18 standalone (Tailwind + Angular Material)
```

Deployed as: **Frontend → Vercel** · **Backend → Render Docker** · **DB → Render Postgres**.

See `DEPLOYMENT.md` for the deploy playbook.

## Architecture at a glance

### Backend (`backend/`)

Clean Architecture, dependencies point inward:

```
NogoYa.Domain          Entities, enums, domain exceptions. Zero deps.
NogoYa.Application     DTOs, services, interfaces, mapping. Depends on Domain.
NogoYa.Infrastructure  EF Core, Npgsql, ClosedXML, audit interceptor.
                       Depends on Application + Domain.
NogoYa.API             Controllers, middleware, Program.cs.
                       Depends on Application + Infrastructure.
```

### Frontend (`frontend/src/app/`)

```
core/                  Models, services, interceptors, i18n (cross-cutting)
layouts/               App shell components
features/              One folder per business feature
  cart/                Public: cart + checkout
  home/                Landing page
  products/            Public: list, card, detail
  stores/              Public: list, detail
  settings/            Admin pages (stores, products, import-products)
shared/                Pure UI components reused across features
```

## Language conventions

| Where | Language |
|-------|----------|
| Code (classes, methods, vars, types, file names, route paths) | **English** |
| Comments and docstrings | **English** |
| UI strings the user sees | **Spanish (Argentina, voseo)** |
| Validation messages shown to the user | **Spanish** |
| Logs and technical error messages | **English** |
| Documentation (`README.md`, `DEPLOYMENT.md`, this folder) | **English** |

## Rules

Read these before writing code in each layer:

- [`rules/backend.md`](./rules/backend.md) — .NET / Clean Architecture rules
- [`rules/frontend.md`](./rules/frontend.md) — Angular / Tailwind / Material rules

If a rule conflicts with what the user asks for in chat, the user wins; offer a
sentence of friction first explaining the trade-off.

## Skills

Reusable instructions for recurring tasks live in [`skills/`](./skills).
Examples to come: scaffolding a new feature, generating a migration, adding a
seeded admin page, etc.

## Quick-start commands

```bash
# Backend
cd backend
dotnet ef migrations add <Name> -p src/NogoYa.Infrastructure -s src/NogoYa.API
dotnet run --project src/NogoYa.API --no-launch-profile --urls "https://localhost:62152"

# Frontend
cd frontend
npm install
npm run start          # http://localhost:4200
```

Connection string for local dev lives in `backend/src/NogoYa.API/appsettings.Local.json`
(gitignored). Use the Render external URL.

## Pull request workflow

- One branch per feature: `feature/<short-name>`.
- Fix branches: `fix/<short-name>`.
- Commits follow Conventional Commits: `feat(area): …`, `fix(area): …`, `refactor(area): …`, `docs(area): …`.
- Never commit secrets, `node_modules`, `bin/`, `obj/`, `.angular/`, or `appsettings.Local.json`.
