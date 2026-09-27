# Returns implementation

Customers open My orders, choose View order details, then Request a return. A single request can contain multiple eligible order lines, with a return quantity from one to the purchased quantity for each selected line. Mixed orders allow standard items while excluding personalized or final-sale items.

Admins open the same order in Admin > Orders. Requested returns can be approved with return-shipping instructions or declined with an explanation. Once an approved return is received and inspected, the admin chooses a demo refund between zero and the selected quantities' paid total. Every decision, timestamp, admin ID and amount is retained in the order's return history. Customers see the decision and notes.

Rules:
- The server records delivery time when an order first changes to Delivered. Requests are accepted through delivery time + 7 days, inclusive.
- Custom-quote orders and name-personalized products are always excluded. The product editor's Final sale setting covers other policy exclusions, including engraving, photo pendants, grillz and made-to-measure items. Classify these products before selling them.
- Product eligibility is snapshotted at purchase. Older orders without that snapshot or a recorded delivery date require manual review; no delivery date or eligibility is guessed.
- Each order can have one request containing multiple selected lines and quantities. Ownership, line index, condition confirmation, reason, photo limits, duplicate requests, optimistic concurrency and refund limits are checked by the API.
- Photos are private storage objects; authorized order reads generate temporary signed URLs. Damaged/incorrect standard-item requests require a photo. Failed uploads/failed saves clean up newly uploaded objects.
- Custom/final-sale damage exceptions, exchanges, cancellations and repairs remain contact-based as described in the supplied policy; the standard return button is absent for custom and name items.

The full supplied policy is at /return-policy, linked from the footer, checkout and order details.

Payments and refunds remain DEMO ONLY. The refund action records a simulation and never calls PayPal. Production blocks the refund simulation; live refund execution needs a payment-provider integration. Approval to send an item back is separate from refund approval after inspection.

Persistence: migration 20260910165943_CustomerReturns adds products.is_final_sale, customer_orders.delivered_at and customer_orders.returns_json. It preserves existing rows and initializes return histories to [].

Validation: 36 order/return unit checks, 26 PostgreSQL integration checks in an isolated schema, and 36 browser checks across 320, 390, 768 and 1366px. Browser fixtures and database test schemas do not modify store orders.

## Multiple items and quantities — September 14, 2026

Customers choose items and quantities in one request. Admins can use Edit return items to add or remove eligible items from the same order and adjust quantities. At least one item must remain; decline the request to reject the whole return. Edits require a customer-facing note, preserve the original customer selection and quantity history, reset the request to Requested, and clear the previous refund agreement. Declined or refunded requests cannot be edited. Eligibility for additions is evaluated at the original request time. Refund caps use the paid unit price multiplied by each selected quantity.

Existing single-item JSON falls back to all purchased units. No schema migration is needed. Current validation: 76 backend tests, 13 PostgreSQL checks in an isolated schema, and browser checks for customer quantities, admin add/remove, multipart submission, mobile overflow and compact admin cards. No store orders were changed by validation. Live payments remain unconnected.

## Direct accept / decline — September 14, 2026

The admin review now opens directly with the editable item/quantity selection, instructions or reason, and total refund amount. Accept validates and saves selection, instructions and amount atomically, retaining the original request and history. Decline requires a reason and does not require a refund amount. There is no separate Confirm decision or refund-agreement screen. Acceptance records an approved refund amount, not a money transfer. Existing historical refund records and compatibility API actions are preserved. Admin orders use a compact single-column list.

Validation: 81 backend tests; browser checks cover direct acceptance with quantities and amount, decline with a reason, customer quantity submission and the vertical order list. Frontend and API builds pass (frontend retains existing product-template and auth stylesheet budget warnings).
