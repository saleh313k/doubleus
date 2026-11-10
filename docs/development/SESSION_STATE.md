# DoubleUs Session State

Last Updated: 2026-11-09

## Current Phase
Phase 1 — Infrastructure and Backend Foundation (owner-approved task list in ROADMAP.md)

## Overall Progress
Phase 0 complete (unmerged). Phase 1: task 1 confirmed; task 2 health endpoints done; database switched from PostgreSQL to SQL Server on owner instruction (D-016).

## Completed Tasks
- Phase 0 docs.
- Pre-coding docs update requested by owner: TENANT_ISOLATION.md, TOKEN_SECURITY.md, RELIABLE_MESSAGING.md, META_ASSUMPTIONS.md refresh (Graph v27.0, Instagram, WhatsApp), DECISIONS D-010..D-015, SYSTEM_ARCHITECTURE/ROADMAP updates.
- Task 1: solution skeleton.
- Task 2: /health/live + /health/ready (TCP probes, optional Redis), HealthEndpointTests (6), .gitattributes, optional docker-compose.yml (SQL Server + Redis), .env.example, infrastructure/scripts/local-sqlserver-setup.sql, docs/deployment/LOCAL_DEVELOPMENT.md. (Originally built on PostgreSQL; switched by D-016.)

## Current Task
Awaiting owner approval of docs/security/RLS_ROLLOUT_PLAN.md (D-017) and D-009 before any Task 3 code. None in progress. Next is Task 3 after owner confirms task 2 (docker compose up + dotnet test).

## Files Modified
global.json, Directory.Build.props, Directory.Packages.props, .editorconfig, .gitignore, DoubleUs.sln, src/**, tests/**, docker-compose.yml, .env.example, .gitattributes, infrastructure/scripts/local-sqlserver-setup.sql, docs/**; API health code in src/DoubleUs.API/Health/

## Database Changes
None.

## Tests
Owner run 1: restore OK, build of src OK, test projects failed CA1707 (fixed, re-run pending). See IMPLEMENTATION_STATUS.md.

## Known Issues
- Tests not yet confirmed passing (task 1 CA1707 fix unconfirmed; task 2 tests never run).
- SQL Server setup script and optional compose file never executed.
- Health readiness is TCP-connectivity only until task 3.
- Authoring environment cannot push to GitHub; work delivered as git bundle/zip.
- Oman eligibility for Meta products still unverified (META_ASSUMPTIONS #5).
- Graph API v27.0 messaging-relevant changes not yet reviewed.

## Important Decisions
D-001..D-016 in DECISIONS.md (D-016: SQL Server replaces PostgreSQL). Phase 1 has no Meta calls, no n8n, no frontend.

## Next Actions
1. Owner: install SQL Server 2025 Developer, run infrastructure/scripts/local-sqlserver-setup.sql (docs/deployment/LOCAL_DEVELOPMENT.md), re-run `dotnet test DoubleUs.sln` (expect 20 tests), report results.
2. Task 3: EF Core SQL Server + RLS security policy + first migration (write D-009 token-encryption ADR and RLS rollout plan first, get owner approval); replace the TCP database health probe with an EF query.
3. Task 4: Identity/auth.

## Git State
- Branch: feature/backend-foundation (based on docs/phase-0-planning)
- Not merged to main. Nothing pushed (no credentials in authoring environment).
