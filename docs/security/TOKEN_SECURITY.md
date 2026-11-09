# Meta Token Storage and Security

Status: design for Phase 1 (storage primitives) and Phase 4 (provider use). No real Meta tokens are handled until Phase 4.

## Storage and encryption at rest
- Tokens live only in `channel_credentials` as ciphertext: AES-256-GCM, random 96-bit nonce per encryption, stored with `key_version`.
- Associated data binds each ciphertext to `tenant_id` + `connected_channel_id` + purpose, so a copied blob cannot be decrypted in another context.
- Key hierarchy: a master key (environment secret or KMS/secret-manager reference) wraps data keys; the mechanism is finalized in ADR D-009 (ASP.NET Data Protection vs custom envelope). The key is dedicated to credentials and never reused for other purposes. Never committed; `.env.example` has placeholders only.
- Key rotation: new key version for new writes; background re-encryption of old rows; old keys retained until none reference them. Procedure documented in docs/security before production.
- Decryption happens only inside the channel adapter at call time. Tokens are never returned by the API, sent over SignalR, passed to n8n or to AI providers, or stored in webhook payloads/audit logs.

## Expiration and health
`token_expires_at` and status (`Active`, `ExpiringSoon`, `Expired`, `NeedsReconnect`, `Revoked`) are tracked. A scheduled health job checks expiry windows and, where the provider supports refresh, refreshes ahead of expiry (token lifetimes and refresh rules are **unverified**, see META_ASSUMPTIONS #9). Failed refresh or provider auth errors move the channel to `NeedsReconnect`, pause AI sending for that channel, and raise a tenant notification. No silent retry loops against invalid tokens.

## Revocation and disconnect
Disconnect (by tenant owner/admin, tenant deletion, or detected revocation) will: mark the channel `Revoked`, call the provider's revoke/unsubscribe endpoints where officially supported (to be verified in Phase 4), **delete the ciphertext**, unsubscribe webhooks, stop queued sends for that channel, and write an audit entry. Provider-side deauthorization/data-deletion callbacks, if required by Meta, are handled the same way.

## Log redaction
- Serilog destructuring policy and a redacting enricher mask any property named like token/secret/authorization/password/appsecret/verify_token/code.
- Outbound `HttpClient` logging handler strips `Authorization` headers and `access_token` query parameters; tokens are sent in headers, not URLs.
- Webhook payload storage redacts configured fields; request bodies are not logged by default.
- Exception messages from provider SDK/HTTP calls are scrubbed before logging.
- Automated tests assert that log output for representative flows contains no token material.
