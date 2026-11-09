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
- **D-009 Proposed** Token encryption mechanism (Data Protection vs AES-GCM envelope): decide in Phase 1.
- **D-010 Accepted (2026-11-09)** Tenant isolation is multi-layer: membership-verified tenant per request, scoped `ITenantContext`, EF filters, **and PostgreSQL RLS as mandatory defense in depth** (non-owner app role, `app.tenant_id` set per transaction). EF filters are not the security boundary. See docs/security/TENANT_ISOLATION.md.
- **D-011 Proposed** Instagram setup: Instagram Login vs Facebook Login. Undecided until Phase 4 doc review and eligibility testing.
- **D-012 Accepted** Reliable messaging via transactional inbox/outbox, idempotency keys, bounded retries, dead-letter with replay. See docs/architecture/RELIABLE_MESSAGING.md.
- **D-013 Accepted** Meta tokens: AES-256-GCM with AAD, key versioning, delete-on-revoke, redacted logs. Mechanism details in D-009. See docs/security/TOKEN_SECURITY.md.
- **D-014 Accepted** Phase 1 contains no Meta API calls and no n8n; Meta facts stay "unverified" until Phase 4.
- **D-015 Accepted** Phase 1 branch `feature/backend-foundation` is based on `docs/phase-0-planning` because main is empty and Phase 0 is not yet merged.
