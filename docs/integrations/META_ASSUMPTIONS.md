# Meta Integration Assumptions and Open Questions

Rule: nothing below may be treated as fact for implementation until its status is **Verified** against current official Meta documentation and, for Phase 4, tested with a real eligible account. Never invent endpoints, permissions or payloads.

| # | Topic | Status | Notes |
|---|---|---|---|
| 1 | Current Graph API version | Verified from Meta changelog (Nov 2026 search) | v26.0 released 2026-07-29; v25.0 released 2026-02-18; older versions expire on a rolling ~2-year schedule. Meta announced that v26.0 restrictions extend to all versions on 2026-10-27, so do not rely on old-version pinning. Re-check in Phase 4. |
| 2 | Instagram messaging API endpoints, permissions, required login flow | Unverified | Read current Instagram Platform docs; record exact permissions and App Review requirements. |
| 3 | WhatsApp Cloud API onboarding (Embedded Signup vs manual) | Unverified | Confirm current onboarding for tech providers / solution partners. |
| 4 | Whether a business in Oman can be onboarded for both products | Unverified, high impact | Check Meta business verification, country eligibility, WhatsApp pricing country support. Do not assume a token means eligibility. |
| 5 | Webhook signature scheme and verification handshake | Unverified | Expected: HMAC-SHA256 of raw body with app secret and a verify-token handshake; confirm per product. |
| 6 | Webhook mTLS change (noted in v25.0 changelog) | Unverified | Review requirement and deadlines before webhook deployment. |
| 7 | WhatsApp customer service window, template categories, pricing model | Unverified | Pricing changes; confirm current rules before designing reply/follow-up logic. |
| 8 | Instagram messaging window and rate limits | Unverified | Confirm allowed reply window and human-agent tag rules. |
| 9 | Token lifetimes and refresh | Unverified | Confirm per token type. |
| 10 | App Review / business verification timelines | Unverified | Start early; may gate the whole MVP. |

## Required before Phase 4 coding
Complete rows 2-10, record the exact doc URLs and access date next to each, and update this file.
