# Decisions (ADR log)

Format: ID, status, decision, reason. Add new entries at the bottom; never delete, supersede instead.

- **D-001 Accepted** Modular monolith + separate worker process. Reason: small team, simpler ops, clear module boundaries keep later extraction possible.
- **D-002 Accepted** Shared DB/shared schema multi-tenancy with `tenant_id`, EF global filters, membership-verified tenant resolution; RLS as defense in depth (scope decided in Phase 1).
- **D-003 Accepted** Meta authentication, tokens, messaging and tenant authorization stay in the ASP.NET Core app; n8n receives only signed, minimal events.
- **D-004 Accepted** Webhooks: verify signature on raw body, persist with unique provider event ID, ack immediately, process in worker.
- **D-005 Accepted** Provider abstractions (`IMessagingChannel`, `IAIProvider`) with mock implementations first so everything is testable without Meta credentials.
- **D-006 Accepted** Dashboard language preference stored separately from AI reply language.
- **D-007 Proposed** Pin the Graph API version in configuration (never hard-coded). v26.0 (2026-07-29) and v27.0 (2026-11-04) exist; target chosen and tested at Phase 4 start.
- **D-008 Proposed** Background jobs via outbox + hosted service vs a job library: decide in Phase 1.
- **D-009 Proposed (recommendation written 2026-11-09, awaiting owner approval)** Token encryption: custom AES-256-GCM envelope (BCL `AesGcm`), tenant+channel bound associated data, per-tenant HKDF-derived keys from a master key held in a secret store, key-id versioning. Details and trade-offs: docs/security/TOKEN_SECURITY.md.
- **D-010 Accepted (2026-11-09)** Tenant isolation is multi-layer: membership-verified tenant per request, scoped `ITenantContext`, EF filters, **and SQL Server RLS as mandatory defense in depth** (non-owner app role, `app.tenant_id` set per transaction). EF filters are not the security boundary. See docs/security/TENANT_ISOLATION.md.
- **D-011 Proposed** Instagram setup: Instagram Login vs Facebook Login. Undecided until Phase 4 doc review and eligibility testing.
- **D-012 Accepted** Reliable messaging via transactional inbox/outbox, idempotency keys, bounded retries, dead-letter with replay. See docs/architecture/RELIABLE_MESSAGING.md.
- **D-013 Accepted** Meta tokens: AES-256-GCM with AAD, key versioning, delete-on-revoke, redacted logs. Mechanism details in D-009. See docs/security/TOKEN_SECURITY.md.
- **D-014 Accepted** Phase 1 contains no Meta API calls and no n8n; Meta facts stay "unverified" until Phase 4.
- **D-015 Accepted** Phase 1 branch `feature/backend-foundation` is based on `docs/phase-0-planning` because main is empty and Phase 0 is not yet merged.
- **D-016 Accepted (owner instruction, 2026-11-09)** The database is **SQL Server 2025 (or Azure SQL Database)**, replacing PostgreSQL; pgvector is replaced by the native `vector` type (EF Core 10 `SqlVector<float>`, `VECTOR_DISTANCE`, per Microsoft docs). Redis stays (optional until used). This deviates from the master spec's stack section by owner decision. Consequences: (1) SQL Server 2025+ is required for vectors; 2022 and earlier have no vector type. (2) Developer edition is dev/test only, so production needs a SQL Server license or Azure SQL (risk R14). (3) Tenant RLS uses security policies + `SESSION_CONTEXT`, not Postgres RLS (docs/security/TENANT_ISOLATION.md); the isolation principles are unchanged. (4) Outbox claims use `UPDLOCK, READPAST`. (5) Vector indexes/`VECTOR_SEARCH` are experimental per Microsoft; the MVP uses exact search filtered by tenant. (6) Supersedes the PostgreSQL-specific parts of D-002 and D-010.
- **D-017 Proposed** Task 3 plan (EF Core, tenant filter, RLS security policies, pooling tests, first migration without business tables): docs/security/RLS_ROLLOUT_PLAN.md.
