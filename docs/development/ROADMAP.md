# Roadmap

Phases run in order; each ends with a PR and **owner approval before the next starts**. Skipping a phase requires a note in DECISIONS.md.

| Phase | Scope | Branch | Exit criteria |
|---|---|---|---|
| 0 | Repo inspection, planning docs, CLAUDE.md, risk register | docs/phase-0-planning | Docs merged; owner approves |
| 1 | Solution skeleton, PostgreSQL + EF Core, Redis, auth (Identity + JWT + refresh rotation), tenants, memberships, invitations, RBAC, Docker Compose, CI, first migrations, tenant-isolation tests | feature/backend-foundation, feature/tenant-management | `docker compose up` works; auth + isolation tests green |
| 2 | React/Vite/TS/Tailwind/shadcn, auth pages, dashboard layout, i18n (EN/AR), RTL, responsive nav | feature/frontend-foundation | Login to dashboard in EN and AR/RTL |
| 3 | Contacts, conversations, messages, SignalR, handoff, mock channel adapters, unified inbox UI | feature/unified-inbox | End-to-end mock conversation incl. handoff |
| 4 | Meta verification, Instagram + WhatsApp connection, webhooks, outbound, delivery status | feature/instagram-integration, feature/whatsapp-integration | Tested with real eligible accounts (otherwise labeled mock-only) |
| 5 | AI provider abstraction, knowledge base, extraction, embeddings, RAG, EN/AR replies, safety/fallback | feature/ai-assistant, feature/knowledge-base | Evaluated EN/AR answers; tenant-scoped retrieval tests |
| 6 | n8n deployment, signed events, workflows, failure/recovery tests, licensing decision documented | feature/automation | Core works with n8n down |
| 7 | Plans, subscriptions, usage enforcement, analytics, admin dashboard | feature/analytics | Server-side limit enforcement tested |
| 8 | Security review, integration tests, performance, Docker verification, staging, launch checklist | feature/production-readiness | Launch checklist complete |

## Parallel track: business/compliance (start now, does not need code)
- Confirm Meta eligibility for Oman businesses (see docs/integrations/META_ASSUMPTIONS.md).
- Create Meta developer app and Business Manager; plan business verification and App Review.
- Decide n8n licensing path.
- Pick initial AI provider(s) and test Arabic quality.

## Phase 1 proposed task breakdown (for approval, not started)
1. Solution + projects + central package management + analyzers + .editorconfig.
2. Docker Compose (postgres+pgvector, redis) and health endpoints.
3. EF Core context, base entity conventions, tenant filter, first migration.
4. Identity, registration/login/logout, refresh rotation, rate limiting.
5. Tenants, members, invitations, role policies.
6. Token encryption service (ADR first).
7. Architecture tests + tenant isolation tests; GitHub Actions CI.
