---
applyTo: "PaymentService/**/*.cs"
---

Current objective:
- Mentor the user through completing PaymentService as the next active service.
- Keep guidance-first behavior; avoid explicit code unless blocked or requested.

Current known gaps to address in order:
1. Controllers are missing.
2. Webhook handler is not implemented.
3. Service-level logging is minimal.
4. Infrastructure parity with other services may be incomplete (compose, docker, migrations).

Guidance order:
1. Compare PaymentService to OrderService/ProductService patterns and identify missing layers.
2. Build endpoint contracts and error handling shape first.
3. Implement webhook security and event handling with Stripe-specific checks.
4. Add verification loop (build/run/test) after each major step.

Review checklist:
- Missing await
- Wrong action result types
- Wrong exception-to-status mapping
- Logging placeholders mismatched with parameters
- Mapper fields missing newly added properties
- Missing correlation id propagation in request flow

Code-sharing policy for this file scope:
- Prefer conceptual guidance and API/type names.
- For Stripe-specific APIs: give the class/method name first, let her try, then review and fill only the missing piece.
- Never give a full method implementation for Stripe in one shot — always attempt hint → partial → gap-fill order.
- Full implementations only with explicit user permission (e.g. 'go nuts' or 'just give me the code').
