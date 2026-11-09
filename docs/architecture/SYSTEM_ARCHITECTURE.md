# System Architecture

Status: Phase 0 design. Nothing here is implemented yet.

## 1. Style
Modular monolith with a separate background worker process. No microservices until a measured need exists (see DECISIONS D-001).

## 2. Solution layout
```
src/
  DoubleUs.Domain/          entities, value objects, enums, domain rules (no dependencies)
  DoubleUs.Application/     use cases, interfaces (IMessagingChannel, IAIProvider, ...), validators, DTOs
  DoubleUs.Infrastructure/  EF Core, Redis, encryption, email, audit, usage metering
  DoubleUs.Integrations/    InstagramMessagingChannel, WhatsAppCloudChannel, MockMessagingChannel
  DoubleUs.AI/              provider adapters, RAG pipeline, language detection, safety layer
  DoubleUs.API/             ASP.NET Core Web API, SignalR hubs, auth, rate limiting
  DoubleUs.Worker/          background jobs (webhook processing, AI replies, retries, usage aggregation)
frontend/                   React + TS + Vite + Tailwind + shadcn/ui
automation/n8n, workflows/  internal workflows exported to VCS
tests/                      Unit, Integration, Architecture
infrastructure/             docker, nginx, scripts
```
Dependency rule (enforced by architecture tests): Domain depends on nothing; Application depends on Domain; Infrastructure/Integrations/AI depend on Application; API and Worker are composition roots.

## 3. Key flows

### Inbound message
1. Provider POSTs to `/webhooks/{channel}`.
2. API verifies signature (raw body, HMAC with app secret) and rejects invalid requests.
3. API stores a `WebhookEvent` (unique key on provider + event/message ID) and acks immediately. Duplicate insert = no-op.
4. Worker picks up the event, normalizes it, upserts Contact + Conversation, persists Message, publishes SignalR update.
5. If AI is `Enabled` for the conversation and channel, Worker enqueues an AI reply job (idempotent per inbound message).

### AI reply
1. Check usage limits and AI state (Paused/HumanHandoff = stop).
2. Detect language; retrieve top-k chunks from pgvector filtered by `TenantId`.
3. Build prompt: system rules + tenant config + retrieved context (as delimited untrusted data) + conversation window.
4. Call `IAIProvider`; parse structured result (text, language, confidence, intent, suggested action, handoff flag, references).
5. Server-side validation of any suggested action; low confidence or sensitive intent triggers handoff.
6. Send via channel adapter, honoring the WhatsApp service window / templates; record delivery events and usage.

### Outbound manual reply
Agent -> API (authorized, tenant-verified) -> outbound message row (idempotency key) -> Worker sends -> status via webhook -> SignalR.

## 4. Multi-tenancy
- Shared database, shared schema, `TenantId` on every tenant-owned row.
- Tenant resolved per request and **verified against the user's `TenantMember` record**; never trusted from the client alone.
- EF Core global query filters by current tenant; worker jobs carry TenantId explicitly and set tenant context.
- PostgreSQL Row-Level Security as defense in depth (evaluated in Phase 1; not the sole control).
- pgvector queries always include the tenant predicate. Integration tests prove cross-tenant access fails.

## 5. Authentication and authorization
ASP.NET Core Identity; short-lived JWT access tokens; rotating refresh tokens stored hashed with reuse detection; rate limiting on auth endpoints. Roles: PlatformAdmin (global), BusinessOwner, BusinessAdmin, Agent (tenant roles via TenantMember). Policy-based authorization with permission checks.

## 6. Channels
`IMessagingChannel` per the spec, with onboarding, token-health and webhook-subscription capabilities split into separate interfaces so adapters stay small. Channels are enabled per tenant and per platform via flags. Provider formats never leak past the adapter: adapters emit a normalized inbound message.

## 7. AI layer
`IAIProvider` (generation + embeddings), selected by configuration and platform admin. RAG: extract, normalize (non-destructive for Arabic), chunk, embed, store in pgvector, retrieve. Safety layer: input isolation, output validation, tool allowlist, PII minimization before provider calls.

## 8. Automation (n8n)
One-way signed events from the core app to n8n (HMAC + timestamp), with timeouts, retries and failure logging. n8n never holds Meta tokens or does tenant auth. Core features degrade gracefully if n8n is down (outbox pattern for automation events).

## 9. Real-time
SignalR hub with per-tenant groups, authorized by membership. Redis backplane when scaled out.

## 10. Reliability
Outbox/inbox tables, idempotent jobs, bounded exponential-backoff retries, dead-letter visibility, health checks (DB, Redis, providers), correlation IDs end to end.

## 11. Deployment
Docker Compose: api, worker, frontend, postgres (pgvector), redis, n8n, nginx. Postgres, Redis and the n8n editor are not publicly exposed. Migrations are applied by an explicit step, never at production startup.

## 12. Open design questions
- RLS adoption scope (Phase 1).
- Job infrastructure: custom outbox + BackgroundService vs Hangfire/Quartz (Phase 1, ADR).
- Embedding model and vector dimension for Arabic + English (Phase 5, needs evaluation).
- Token encryption: ASP.NET Data Protection vs AES-GCM with envelope keys (Phase 1, ADR).
