# Database Design

Status: Phase 0 draft. PostgreSQL, EF Core migrations, UUID PKs, UTC `timestamptz`. All tenant-owned tables carry `tenant_id` (indexed, FK to `tenants`) and are covered by an EF global filter.

## Conventions
- snake_case names; `created_at` / `updated_at` on all rows.
- Soft delete only where retention or audit requires it; FK `ON DELETE` behavior chosen and documented per relation.
- Tokens and secrets stored encrypted; never logged.

## Identity and tenancy
| Table | Notes |
|---|---|
| users | Identity user; global, not tenant-owned |
| refresh_tokens | hashed token, family id, expiry, revoked / replaced-by (reuse detection) |
| tenants | name, status (Active/Suspended), default locale |
| tenant_members | user_id, tenant_id, role; unique(user_id, tenant_id) |
| tenant_invitations | email, role, token hash, expiry, status |
| roles / permissions | tenant role-to-permission mapping (PlatformAdmin is a global role) |

## Billing and usage
plans (limits stored as data, no hard-coded prices), subscriptions (tenant_id, plan_id, status, period), usage_counters (tenant_id, metric, period, value; unique per tenant+metric+period), ai_usage_records (tenant, conversation, provider, model, tokens in/out, cost estimate). WhatsApp provider charges are tracked separately from AI provider charges.

## Channels
connected_channels (tenant_id, type, external_account_id, status, ai_enabled, token_expires_at); channel_credentials (connected_channel_id, encrypted_payload, key_version).

## Messaging
| Table | Key points |
|---|---|
| contacts | tenant_id, channel, external_id, display_name, phone (if permitted), preferred_language, first/last_interaction_at; unique(tenant_id, channel, external_id); no automatic cross-channel merge |
| conversations | tenant_id, contact_id, connected_channel_id, status (Open/Pending/Resolved/Archived), ai_state (Enabled/Paused/HumanHandoff), assigned_agent_id, customer_language, unread_count, last_message_at; index (tenant_id, status, last_message_at desc) |
| messages | conversation_id, direction, sender_type (Customer/AI/Agent/System), provider_message_id, body, delivery_status; unique(connected_channel_id, provider_message_id) |
| message_attachments | message_id, type, storage ref, size, mime |
| message_delivery_events | message_id, status, provider_timestamp, error code/class |
| internal_notes | conversation_id or contact_id, author_id, body |
| webhook_events | provider, provider_event_id, payload, status, attempts, received_at; unique(provider, provider_event_id) |

## AI and knowledge
ai_configurations (tenant_id, assistant name, tone, languages, instructions, escalation rules, per-channel enable); knowledge_documents (tenant_id, source type, status, checksum); knowledge_chunks (tenant_id, document_id, content, language, embedding vector(N), token_count). Vector index (HNSW or IVFFlat) always combined with the tenant predicate. N is fixed by the chosen embedding model (ADR pending).

## Platform
audit_logs (actor, tenant_id nullable, action, target, metadata, ip, at); automation_events (outbox: type, payload, status, attempts, next_attempt_at); failed-job tracking (own table or library tables, per ADR).

## Indexing priorities
Conversation list/filter; messages by conversation + time; webhook idempotency uniques; chunks by tenant + vector; usage counters by tenant + period.

## Data lifecycle
Per-tenant retention windows; disconnecting a channel revokes and destroys its credentials; a deletion request purges tenant-owned rows, chunks and stored files, keeping only an audit record.
