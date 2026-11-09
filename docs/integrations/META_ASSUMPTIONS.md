# Meta Integration Assumptions and Open Questions

Rule: nothing below may be treated as fact for implementation until its status is **Verified** against current official Meta documentation and, for Phase 4, tested with a real eligible account. Never invent endpoints, permissions or payloads.

| # | Topic | Status | Notes (reviewed 2026-11-09) |
|---|---|---|---|
| 1 | Current Graph API version | Partially verified | v26.0 released 2026-07-29 (Meta changelog). **v27.0 released 2026-11-04**: reported by secondary coverage that links Meta's developer blog post "Introducing Graph API v27.0 and Marketing API v27.0"; the v27.0 changelog itself was not read in full. Meta states v21.0 is removed 2027-01-21 and v22.0 on 2027-05-20. Messaging-relevant v27 breaking changes: **not yet reviewed**. Docs examples seen still use v25.0 paths. Version is a config value (ADR D-007); pick and test the target at Phase 4 start. |
| 2 | Instagram messaging: two API setups exist (Instagram API with Instagram Login on graph.instagram.com, scopes `instagram_business_basic` + `instagram_business_manage_messages`; Instagram API with Facebook Login on graph.facebook.com) | Partially verified from official docs excerpts | Instagram Login setup does not require a linked Facebook Page. Business and Creator accounts supported. Send via `/<IG_ID>/messages` with `recipient:{id:<IGSID>}`; text UTF-8, max 1000 bytes; group messaging unsupported. Webhook fields seen: messages, messaging_postbacks, messaging_seen, messaging_reactions, messaging_referral, messaging_optins. Which setup to use: undecided (ADR D-011). Full doc read required in Phase 4. |
| 3 | Instagram access levels | Partially verified | Standard Access for apps serving accounts you own/manage; Advanced Access (App Review + Business Verification) for serving other businesses, which a SaaS needs. Instagram messaging inbox: API messages count as Read only after a reply. |
| 4 | WhatsApp Cloud API onboarding | Partially verified | Embedded Signup is the multi-customer flow, requires session logging; **Embedded Signup v2 deprecated 2026-10-15, must use v4**. Webhook subscription per customer WABA via `POST /{WABA_ID}/subscribed_apps` (from third-party write-up, unverified against official reference). Tech Provider / Solution Partner requirements not reviewed. |
| 5 | Whether a business in **Oman** can be onboarded for each product, incl. pricing/payment country support | **Unverified, high impact** | No official source reviewed. Do not assume a token implies eligibility. Needs a real test or written confirmation from Meta. |
| 6 | Webhook signature and handshake | Partially verified | Official/third-party sources agree: `X-Hub-Signature-256` = `sha256=` + HMAC-SHA256 of the **raw body** with the app secret; GET verification uses `hub.mode`, `hub.verify_token`, `hub.challenge`. Instagram signature detail per setup to confirm. |
| 7 | Webhook mTLS CA change (Meta signs mTLS certs with a new Meta-owned CA from 2026-03-31, per v25 blog) | Unverified impact | Applies if mTLS is enabled for the webhook endpoint; review in Phase 4. |
| 8 | WhatsApp 24-hour customer service window / templates / pricing | Partially verified | 24h window opens only when a user messages an already-onboarded business; outside it only approved templates. Pricing model and categories not reviewed. |
| 9 | Token lifetimes, refresh, revocation endpoints | Unverified | Long-lived Instagram tokens exist; exact lifetimes/refresh/revoke endpoints to be read before Phase 4. |
| 10 | App Review / Business Verification timelines and data-handling obligations (deauthorize, data deletion callbacks) | Unverified | Start early; may gate the MVP. |
| 11 | Instagram messaging window and human-agent rules | Unverified | Docs mention a Human Agent feature and 24-hour window for self messaging; exact rules to confirm. |

**Policy:** no production Meta integration code is written from rows marked Unverified or Partially verified. Phase 1 contains no Meta calls.

## Required before Phase 4 coding
Complete rows 2-10, record the exact doc URLs and access date next to each, and update this file.
