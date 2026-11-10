# Reliable Message Processing

Pattern: **transactional inbox + transactional outbox** in SQL Server, processed by the worker. Delivery is at-least-once; correctness comes from idempotency.

## Inbound (inbox)
1. Webhook handler verifies the signature on the raw body, then inserts a `webhook_events` row with unique key (provider, provider_event_id or derived message ID). Unique violation = duplicate, acknowledged without reprocessing. Respond 200 quickly.
2. Worker claims pending events (`SELECT ... WITH (UPDLOCK, READPAST, ROWLOCK)`, lease timeout), normalizes, and in one transaction upserts contact/conversation, inserts the message (unique on channel + provider_message_id), and writes follow-up outbox rows (SignalR notification, AI reply job). A crash rolls back; the event is reclaimed after lease expiry.

## Outbound (outbox)
- Sending a reply inserts an `outbound_messages`/outbox row in the same transaction as the domain change, with a client-generated idempotency key.
- The worker claims rows, calls the channel adapter, and records provider message ID and status. Delivery webhooks update status.
- **Residual risk:** a crash after the provider accepts but before the DB update could cause a resend. Mitigations: persist an "attempting" state with attempt ID before the call, reconcile through delivery webhooks/provider message lookup where supported, and never auto-retry non-idempotent sends once state is ambiguous without a reconcile check. This risk is documented, not eliminated.

## AI jobs
One AI reply job per inbound message (unique key = inbound message ID), so duplicates cannot cause duplicate AI replies. Jobs re-check conversation AI state (Paused/HumanHandoff) and usage limits just before sending.

## Retries and dead-letter
- Error classification: transient (network, 5xx, rate-limit) vs permanent (auth, invalid recipient, policy/window violation).
- Bounded retries with exponential backoff and jitter; rate-limit responses honor provider retry hints.
- After max attempts or on a permanent error the row moves to `DeadLettered` with reason, attempt history and timestamps. Dead-lettered items are visible to tenant admins (their own) and platform admins (metadata only), and can be manually replayed after the cause is fixed.
- Poison events do not block the queue; ordering is only guaranteed per conversation where required.
- Health checks and metrics expose queue depth, oldest pending age and dead-letter count.

## Phase scoping
Phase 1 builds only the generic outbox/inbox primitives and their tests if approved as part of the foundation task list; channel-specific processing arrives in Phases 3-4.
