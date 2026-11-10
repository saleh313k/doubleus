# CLAUDE.md — DoubleUs project instructions

DoubleUs is a real commercial multi-tenant SaaS (AI replies on Instagram DMs + WhatsApp Business, English/Arabic, initial market Oman). Implement real working code; do not just describe it. Work in small, verifiable, separately committed phases. **The repository is the source of truth for progress.**

## Every session — start here
1. Read this file, `docs/development/IMPLEMENTATION_STATUS.md`, `docs/development/SESSION_STATE.md`, relevant parts of `docs/development/ROADMAP.md`.
2. `git status`, `git branch --show-current`, `git log --oneline -10`.
3. Inspect relevant source; run relevant tests.
4. Identify the exact next incomplete task and continue from it.
5. Never restart, regenerate, or recreate completed work. If docs and code disagree, trust verified code and reconcile the docs.

## Context checkpointing
At roughly 65–70% context use: stop starting new work, finish the current small task, update `SESSION_STATE.md` + `IMPLEMENTATION_STATUS.md` (completed / partial / remaining, file paths, symbols, bugs, test results, exact next action), and commit verified work. Never silently abandon an operation.

## Scope control
- One coherent unit of work at a time; never multiple major phases in one session.
- No unrequested refactors; don't touch working components unless the task requires it.
- **Phase gate:** after finishing a phase, stop and wait for the owner's approval before the next.

## Local environment (owner's machine)
- Windows + PowerShell. .NET SDK 10. Docker is NOT used.
- Database: SQL Server 2025 Express, named instance `localhost\SQLEXPRESS`, database `DoubleUs`, SQL logins `doubleus_owner` (migrations/DDL) and `doubleus_app` (runtime, DML only). Mixed mode is enabled. Setup script: `infrastructure/scripts/local-sqlserver-setup.sql`; guide: `docs/deployment/LOCAL_DEVELOPMENT.md`.
- Connection strings and passwords come from environment variables or user-secrets, never from the repo. Test variables: `DOUBLEUS_TEST_OWNER_CONNECTION`, `DOUBLEUS_TEST_APP_CONNECTION`. Runtime: `ConnectionStrings__Default` (app login), `ConnectionStrings__Migrations` (owner login).
- Before every commit run `dotnet build DoubleUs.sln` and `dotnet test DoubleUs.sln` and report real results. Never claim a check passed that you did not run.
- Package versions: look up current versions on NuGet yourself and add them to `Directory.Packages.props` (central package management; no inline versions).
- Redis is optional and unused in Phase 1.

## Current task pointer
Phase 1 is approved with a fixed 8-task list (docs/development/ROADMAP.md). Task 3 is specified in `docs/security/RLS_ROLLOUT_PLAN.md` (approved). Do not start Task 4 before the owner approves Task 3.

## Architecture (see docs/architecture)
Modular monolith. Projects: API, Application, Domain, Infrastructure, Integrations, AI, Worker (+ frontend, automation, tests, infrastructure, docs). Dependency direction: API → Application → Domain; Infrastructure/Integrations/AI implement Application interfaces. Keep provider-specific (Meta, AI vendor) logic inside adapters. Controllers stay thin.

## Non-negotiables
- **Tenant isolation:** never trust a client-supplied tenant ID; verify membership. Every tenant-owned entity has `TenantId` + global query filter. Isolation tests are mandatory.
- **Official Meta APIs only.** No scraping, browser automation, WhatsApp Web automation, cookie extraction. Never invent endpoints/permissions/payloads: verify against current official docs and record in `docs/integrations/META_ASSUMPTIONS.md`.
- **Do not claim an integration works** unless tested against the real provider; otherwise label it mock-only.
- **Webhooks:** verify signatures, idempotent by provider event/message ID, ack fast, process async.
- **n8n** is internal automation only: no tenant auth, Meta tokens, billing, or critical delivery logic; signed internal events; core app works without it; editor never exposed to customers.
- **AI safety:** customer text and documents are untrusted; no LLM-generated SQL/shell/code execution; tool allowlist; server-side validation of AI actions; AI may not promise bookings/refunds/discounts/payments; don't fabricate business facts; hand off when uncertain.
- **Secrets:** never commit keys, tokens, passwords, certs, or customer data. Tokens encrypted at rest with a dedicated key.
- No schema creation at startup in production; use EF Core migrations. UUID PKs, UTC timestamps.
- No placeholder implementations or TODOs for required MVP features.

## Git
Feature branches (`feature/backend-foundation`, `feature/unified-inbox`, …), small descriptive commits, never commit knowingly broken code, never force-push or rewrite shared history, PR per substantial phase, keep `main` stable.

## Coding standards
Clear names, async everywhere, pass `CancellationToken`, DI, validate external input, structured logging (Serilog + correlation IDs), document non-obvious decisions in `DECISIONS.md`. Ask the owner a question only when a decision genuinely blocks safe implementation.
