# Implementation Status

Last updated: 2026-11-09

| Phase | Status |
|---|---|
| 0 Planning | Complete (branch `docs/phase-0-planning`, not yet merged) |
| 1 Backend foundation | In progress: task 2 of 8 code-complete on `feature/backend-foundation` |
| 2-8 | Not started |

## Phase 1 tasks (scope fixed by owner approval 2026-11-09)
| # | Task | Status |
|---|---|---|
| 1 | Solution, projects, central package management, analyzers, .editorconfig | **Done, tests confirmed by owner** |
| 2 | Health endpoints + local database setup | **Health endpoints done; owner ran 18/18 tests green before the SQL Server change. Database setup (SQL Server) not yet done by owner** |
| 3 | EF Core (SQL Server), tenant filter + RLS security policy, first migration | Not started |
| 4 | Identity, auth, refresh rotation, rate limiting | Not started |
| 5 | Tenants, members, invitations, role policies | Not started |
| 6 | Token encryption service + log redaction | Not started |
| 7 | Inbox/outbox/dead-letter primitives | Not started |
| 8 | Architecture + tenant isolation tests, GitHub Actions CI | Not started |

## Change 2026-11-09: PostgreSQL replaced by SQL Server (D-016, owner instruction)
Changed: DB provider decision, docs (TENANT_ISOLATION, DATABASE_DESIGN, RELIABLE_MESSAGING, SYSTEM_ARCHITECTURE, ROADMAP, RISK_REGISTER R14/R15, LOCAL_DEVELOPMENT), health options (`Dependencies:Database` port 1433; Redis now optional via `Dependencies:Redis:Required`, default false), health tests (now 6, was 4), `docker-compose.yml` (SQL Server + Redis, optional), `.env.example`, new `infrastructure/scripts/local-sqlserver-setup.sql`; removed `infrastructure/docker/postgres/`.
Verified here (SDK 10.0.114, no SQL Server available): API builds with 0 warnings/0 errors; API run against local TCP listeners gave the expected live/ready results in 5 scenarios (database down/up, Redis optional/required/up/down).
Microsoft docs confirm (read via search): EF Core 10 supports the SQL Server `vector` type (`SqlVector<float>`, `VECTOR_DISTANCE`) on SQL Server 2025 and Azure SQL only; vector indexes/`VECTOR_SEARCH` are experimental.

## NOT verified
- The updated `HealthEndpointTests` (6 tests) have not been run; owner's last run (18 passed) was the Postgres-era 4 health tests.
- `infrastructure/scripts/local-sqlserver-setup.sql` and `docker-compose.yml` have never been executed; the `mcr.microsoft.com/mssql/server:2025-latest` tag is unchecked.
- SQL Server RLS behavior with connection pooling is unproven (mandatory test in task 3).
- No Meta integration, no Meta calls, no database schema exist yet.
