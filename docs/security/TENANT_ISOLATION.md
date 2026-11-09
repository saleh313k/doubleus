# Tenant Isolation Strategy

Principle: **EF Core global query filters are one layer, never the security boundary.** Isolation is enforced at four independent layers; a bug in one must not expose data.

## Layers
1. **Request authorization.** The active tenant is chosen per request (`X-Tenant-Id` header / route). It is verified against `tenant_members` on every request; the JWT carries the user identity only and is never the authority for tenant access. Suspended tenants and removed members fail closed.
2. **Application scope.** A single `ITenantContext` (scoped) holds the verified tenant. Tenant-owned repositories/queries require it. There is no ambient default tenant: if it is unset, tenant-owned queries throw.
3. **EF Core global query filters** on every `ITenantOwned` entity, plus write-side checks (SaveChanges interceptor rejects inserts/updates whose `TenantId` differs from the context).
4. **PostgreSQL Row-Level Security.** Adopted in Phase 1 as mandatory defense in depth (ADR D-010). The application connects as a non-owner role without `BYPASSRLS`; each transaction runs `SELECT set_config('app.tenant_id', @id, true)`. RLS policies on tenant tables compare `tenant_id` to that setting. Migrations run under a separate privileged role. If no tenant is set, policies return no rows.

## Specific surfaces
- **Raw SQL:** allowed only inside Infrastructure through `ITenantSqlExecutor` (parameterized, runs inside a tenant-scoped transaction so RLS applies). `FromSqlRaw`/`ExecuteSqlRaw` anywhere else fails an architecture test. Raw SQL must still state the tenant predicate explicitly; RLS is the backstop.
- **Background jobs / worker:** every job payload carries `TenantId`. The job runner creates a new DI scope, sets `ITenantContext` from the payload (and re-validates the tenant is active), then sets the RLS setting. A job with no TenantId fails closed and goes to dead-letter. Cross-tenant maintenance jobs (usage aggregation, token health) iterate tenants one at a time with the same scoping.
- **Administrative operations:** PlatformAdmin endpoints live in a separate route group and policy, use an explicit `IAdminScope` (never `IgnoreQueryFilters` ad hoc), run under a dedicated DB role with narrow privileges (tenant metadata, plans, health), expose no message content by default, and write an audit log entry per action. `IgnoreQueryFilters` is banned outside an allowlisted class (architecture test).
- **Tenant switching:** switching is just sending a different verified `X-Tenant-Id`; no server-side "current tenant" session state. SignalR connections join groups only after membership verification and are re-validated on tenant change; membership removal disconnects the user from that tenant's groups.
- **Caches and Redis:** all keys are prefixed by tenant ID. **pgvector:** every retrieval query includes the tenant predicate and runs under RLS.
- **Files/knowledge storage:** paths include the tenant ID; access goes through tenant-checked services.

## Required tests (Phase 1 onward)
Cross-tenant read, update, delete and list by ID guessing; user in tenant A sending tenant B's ID; removed member loses access; suspended tenant blocked; job without TenantId rejected; filter bypass attempts; RLS verified with filters disabled (proves layer 4 alone blocks access); raw SQL helper scoping; admin endpoint access control; architecture tests for banned APIs.
