# Decisions (ADR log)

Format: ID, status, decision, reason. Add new entries at the bottom; never delete, supersede instead.

- **D-001 Accepted** Modular monolith + separate worker process. Reason: small team, simpler ops, clear module boundaries keep later extraction possible.
- **D-002 Accepted** Shared DB/shared schema multi-tenancy with `tenant_id`, EF global filters, membership-verified tenant resolution; RLS as defense in depth (scope decided in Phase 1).
- **D-003 Accepted** Meta authentication, tokens, messaging and tenant authorization stay in the ASP.NET Core app; n8n receives only signed, minimal events.
- **D-004 Accepted** Webhooks: verify signature on raw body, persist with unique provider event ID, ack immediately, process in worker.
- **D-005 Accepted** Provider abstractions (`IMessagingChannel`, `IAIProvider`) with mock implementations first so everything is testable without Meta credentials.
- **D-006 Accepted** Dashboard language preference stored separately from AI reply language.
- **D-007 Proposed** Pin the Graph API version in configuration (not hard-coded); initial target v26.0 per Meta's changelog (released 2026-07-29), to be re-verified in Phase 4.
- **D-008 Proposed** Background jobs via outbox + hosted service vs a job library: decide in Phase 1.
- **D-009 Proposed** Token encryption mechanism (Data Protection vs AES-GCM envelope): decide in Phase 1.
