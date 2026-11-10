# Database Design

Status: Phase 0 draft, revised for SQL Server (D-016). SQL Server 2025 / Azure SQL, EF Core migrations, UUID PKs, UTC timestamps. Table names below are logical; EF Core default naming applies. All tenant-owned tables carry `tenant_id` (indexed, FK to `tenants`) and are covered by an EF global filter and an RLS security policy.

## Conventions
- Logical names in snake_case here; physical names follow EF Core defaults. `created_at` / `updated_at` on all rows.
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
ai_configurations (tenant_id, assistant name, tone, languages, instructions, escalation rules, per-channel enable); knowledge_documents (tenant_id, source type, status, checksum); knowledge_chunks (tenant_id, document_id, content, language, embedding vector(N), token_count). Search is exact `VECTOR_DISTANCE` always combined with the tenant predicate (per-tenant sets are small). SQL Server's approximate vector index is documented by Microsoft as experimental, so it is not used for the MVP until benchmarked. N is fixed by the chosen embedding model (ADR pending); EF Core 10 maps it as `SqlVector<float>` with column type `vector(N)`.

## Platform
audit_logs (actor, tenant_id nullable, action, target, metadata, ip, at); automation_events (outbox: type, payload, status, attempts, next_attempt_at); failed-job tracking (own table or library tables, per ADR).

## Indexing priorities
Conversation list/filter; messages by conversation + time; webhook idempotency uniques; chunks by tenant + vector; usage counters by tenant + period.

## Data lifecycle
Per-tenant retention windows; disconnecting a channel revokes and destroys its credentials; a deletion request purges tenant-owned rows, chunks and stored files, keeping only an audit record.

## SQL Server specifics
- **Types:** `uniqueidentifier` keys, `datetime2` columns holding UTC, `nvarchar` for all text (Arabic and emoji), database collation `Latin1_General_100_CI_AS_SC`.
- **Key generation:** random GUIDs fragment clustered indexes; use sequential keys (application-generated UUIDv7 or `NEWSEQUENTIALID()`), decided in task 3.
- **Idempotency uniques:** a SQL Server unique index allows only one NULL, so nullable unique columns (for example optional provider IDs) use filtered unique indexes.
- **Vector support** requires SQL Server 2025 or Azure SQL Database; SQL Server 2022 and earlier cannot be used.
