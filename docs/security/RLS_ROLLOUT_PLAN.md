# Phase 1 Task 3 Plan: EF Core, Tenant Filter, SQL Server RLS, First Migration

Status: **APPROVED by owner 2026-11-10 (D-017). No Task 3 code exists yet.** Implements TENANT_ISOLATION.md layers 2-4 on SQL Server (D-010, D-016).

## Honest constraint
Task 3 needs NuGet packages (EF Core SQL Server provider and design-time tooling). The authoring sandbox cannot reach NuGet, so **it cannot compile or run Task 3 code**; every build/test result must come from the owner's machine (or CI). Decision (owner, 2026-11-10): Task 3 onward is implemented with Claude Code inside the local clone, where `dotnet build/test` and the local SQL Server are available. See docs/development/CLAUDE_CODE_HANDOFF.md.

## Scope (and non-scope)
In: `Domain` base types and `ITenantOwned`; `ITenantContext`; `DoubleUsDbContext` (EF Core 10, SQL Server); tenant query filter; SaveChanges tenant stamping/guard; connection interceptor that sets the tenant session context; RLS objects and a reusable migration helper; first migration (`Tenants` table + `Security` schema + predicate function); real database health probe; isolation tests against the local SQL Server.
Out: users/auth (task 4), memberships/invitations (task 5), token encryption (task 6), inbox/outbox tables (task 7), any business table such as contacts or conversations (later phases; they will call the same RLS helper).
The first migration deliberately contains no tenant-owned business table. Isolation is proven with a probe table created by the test fixture through the same helper.

## Design
1. **Domain:** `Entity` (Guid Id, CreatedAt/UpdatedAt UTC), `ITenantOwned { Guid TenantId }`, `Tenant` (not tenant-owned; it is the root).
2. **Application:** `ITenantContext { Guid? TenantId }` plus a scope that sets it. No ambient default; unset means no tenant.
3. **Filter (layer 3):** one model-building rule applies `HasQueryFilter(e => e.TenantId == tenantContext.TenantId)` to every `ITenantOwned` type. A `SaveChanges` interceptor stamps `TenantId` on inserts and rejects any insert/update whose `TenantId` differs from the context.
4. **RLS (layer 4):** schema `Security`; inline schema-bound function `Security.fn_TenantAccessPredicate(@TenantId uniqueidentifier)` returning a row only when `@TenantId` equals `SESSION_CONTEXT(N'tenant_id')`; a migration helper `ApplyTenantSecurity(table)` creating, per tenant-owned table, a `SECURITY POLICY` with a FILTER predicate plus BLOCK AFTER INSERT and BLOCK AFTER UPDATE (filter covers reads, updates and deletes of other tenants' rows). Policies are `STATE = ON, SCHEMABINDING = ON`.
5. **Session context:** a `DbConnectionInterceptor` runs `sp_set_session_context @key = N'tenant_id', @value = <tenant>, @read_only = 1` every time a connection is opened for a scope that has a tenant. With no tenant, nothing is set and RLS returns no rows.
6. **Two connection strings:** `ConnectionStrings:Default` uses `doubleus_app` (runtime, DML only). `ConnectionStrings:Migrations` uses `doubleus_owner` (DDL, policies). Migrations are never applied at startup; they run through an explicit step.
7. **Cross-tenant/admin access:** no "bypass" flag in the predicate. Platform-admin operations use a separate narrow login limited to non-tenant tables and metadata views (created when first needed, task 5+).
8. **Health:** the TCP database probe is replaced by an EF `CanConnectAsync` using the runtime login (Redis stays TCP/optional).

## Pooling: proven by test, with a decision rule
SESSION_CONTEXT is connection-scoped, and reports exist of pooled connections keeping session values in some client stacks. With `@read_only = 1`, a stale value would make the next `sp_set_session_context` fail (fail-closed) or, worse, leak. Before relying on pooling, tests must show with `Microsoft.Data.SqlClient` pooling ON that: tenant A's context is gone (or replaceable) when tenant B's request reuses the physical connection, and a request with no tenant sees nothing.
Decision rule if a test fails: (a) disable pooling for the app connection (`Pooling=false`, slower, simplest), or (b) explicitly clear/reset context on connection return, or (c) use `@read_only = 0` plus the raw-SQL ban. The owner is consulted before choosing (c).

## Tests (SQL Server required; run against the local instance)
Configured by environment variables `DOUBLEUS_TEST_OWNER_CONNECTION` and `DOUBLEUS_TEST_APP_CONNECTION` (never committed). When unset the SQL tests skip with an explicit message; CI must set them and fail if any skipped.
1. Tenant A cannot read tenant B's rows (EF filter ON).
2. Same query with `IgnoreQueryFilters()` still returns only own rows (proves RLS alone).
3. Raw SQL `SELECT *` through the app login returns only the current tenant's rows.
4. No tenant set returns zero rows.
5. BLOCK predicate: insert and update targeting another tenant are rejected.
6. Delete/update by guessed ID of another tenant's row affects 0 rows.
7. `@read_only` holds: the app cannot change the tenant mid-connection.
8. Pooling tests above (A then B reusing a connection; none after release).
9. `doubleus_app` cannot create tables, alter or drop the policy or the function.
10. Coverage test: every `ITenantOwned` entity type in the EF model has a security policy (queried from `sys.security_predicates`); a new tenant-owned table without RLS fails the build.
11. Architecture tests: `FromSqlRaw`/`ExecuteSqlRaw`/`IgnoreQueryFilters` banned outside the allowlist.

## Definition of done for Task 3
Migration applies cleanly via the owner login; all tests above green on the owner's machine; docs (TENANT_ISOLATION, DATABASE_DESIGN, LOCAL_DEVELOPMENT) updated with results; pooling decision recorded as an ADR.

## Decisions log for this plan
- (resolved) Plan and D-009 approved.
- (resolved, D-018) Primary keys: client-generated sequential GUIDs via EF Core's SQL Server-aware generator, not raw UUIDv7.
- (resolved) Implementation continues locally with Claude Code.
