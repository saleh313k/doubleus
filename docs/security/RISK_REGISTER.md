# Risk Register

| ID | Risk | Impact | Likelihood | Mitigation | Owner/Phase |
|---|---|---|---|---|---|
| R1 | Meta eligibility or App Review blocks Instagram/WhatsApp for Oman businesses | Critical (MVP blocked) | Unknown | Verify early (parallel track), mock adapters so engineering continues, fallback channel plan | Owner, now |
| R2 | n8n Sustainable Use License may not permit this commercial use | High | Medium | Read current license; keep n8n strictly internal; obtain commercial license or replace with an in-app job engine before launch | Owner, Phase 6 |
| R3 | Cross-tenant data leak (SQL, vector retrieval, SignalR, jobs) | Critical | Low-Medium | Query filters, membership checks, RLS, mandatory isolation tests, tenant predicate in vector queries | Phase 1+ |
| R4 | Prompt injection via customer messages or uploaded documents | High | High | Untrusted-input framing, tool allowlist, server-side action validation, no secrets in prompts | Phase 5 |
| R5 | AI fabricates prices/policies | High (trust, legal) | Medium | Retrieval-grounded answers, confidence threshold, handoff, evaluation set | Phase 5 |
| R6 | Weak Arabic retrieval/answer quality | High (core market) | Medium | Non-destructive normalization, Arabic eval set, model comparison before choosing | Phase 5 |
| R7 | OAuth token leakage | Critical | Low | Encrypted at rest, dedicated key, redaction in logs, rotation docs | Phase 1/4 |
| R8 | Duplicate webhooks cause duplicate replies | Medium | High | Unique event IDs, idempotent jobs, outbound idempotency keys | Phase 3/4 |
| R9 | WhatsApp window/template rules violated | High (account suspension) | Medium | Enforce window in adapter, template support, policy review | Phase 4 |
| R10 | Meta API version churn | Medium | High | Configurable pinned version, contract tests, changelog monitoring | Phase 4+ |
| R11 | Personal data sent to AI providers (privacy, data residency) | High | Medium | Data minimization, provider DPA review, configurable/self-hosted providers | Phase 5 |
| R12 | Single-developer bandwidth / scope creep | Medium | High | Small phases, phase gates, strict MVP scope | Ongoing |
| R13 | Provider costs (WhatsApp + AI) exceed plan revenue | Medium | Medium | Usage metering, separate cost tracking, enforced limits | Phase 7 |
| R14 | SQL Server licensing/hosting cost for production (Developer edition is dev/test only; Express has size/resource limits) | High (margin, launch) | Medium | Decide edition/host early: Azure SQL Database or licensed SQL Server; model cost per tenant in plan pricing | Owner, before Phase 8 |
| R15 | SQL Server vector features immature (vector index experimental) and RLS not yet proven with pooled connections | Medium-High | Medium | Exact search filtered by tenant for MVP; benchmark before ANN; mandatory pooling/RLS tests in Phase 1 task 3 | Phase 1/5 |
