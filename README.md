# DoubleUs

Multi-tenant, AI-powered customer communication and sales automation SaaS. Businesses in Oman connect their official Instagram Professional and WhatsApp Business accounts, and an AI assistant answers customer messages in English and Arabic using each business's own knowledge base.

**Status:** Phase 0 (planning). No application code yet. See [`docs/development/IMPLEMENTATION_STATUS.md`](docs/development/IMPLEMENTATION_STATUS.md).

## Planned stack
.NET 10 / ASP.NET Core · EF Core · PostgreSQL + pgvector · Redis · SignalR · React + TypeScript + Vite + Tailwind + shadcn/ui · self-hosted n8n (internal only) · Docker Compose · GitHub Actions

## Documentation map
| Area | File |
|---|---|
| Project rules for AI/dev sessions | [CLAUDE.md](CLAUDE.md) |
| Architecture | [docs/architecture/SYSTEM_ARCHITECTURE.md](docs/architecture/SYSTEM_ARCHITECTURE.md) |
| Database | [docs/architecture/DATABASE_DESIGN.md](docs/architecture/DATABASE_DESIGN.md) |
| Roadmap | [docs/development/ROADMAP.md](docs/development/ROADMAP.md) |
| Status | [docs/development/IMPLEMENTATION_STATUS.md](docs/development/IMPLEMENTATION_STATUS.md) |
| Session state | [docs/development/SESSION_STATE.md](docs/development/SESSION_STATE.md) |
| Decisions (ADR log) | [docs/development/DECISIONS.md](docs/development/DECISIONS.md) |
| Meta integration assumptions & open questions | [docs/integrations/META_ASSUMPTIONS.md](docs/integrations/META_ASSUMPTIONS.md) |
| Risk register | [docs/security/RISK_REGISTER.md](docs/security/RISK_REGISTER.md) |

## Security
Never commit secrets, tokens, or customer data. Use `.env.example` placeholders only.
