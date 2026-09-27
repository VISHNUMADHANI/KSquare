# PayPal Checkout setup

PayPal hosted checkout is connected to the bag and approved custom quotes. The backend creates and captures the payment; a browser redirect alone never marks an order paid.

## Add your credentials

Edit `KsSquare-Backend/.env` (never commit this file):

```dotenv
PAYPAL_ENVIRONMENT=sandbox
PAYPAL_CLIENT_ID=
PAYPAL_CLIENT_SECRET=
PAYPAL_WEBHOOK_ID=
PAYPAL_MERCHANT_ID=
PAYPAL_RETURN_BASE_URL=http://localhost:4200
```

1. Open https://developer.paypal.com/dashboard/applications/sandbox and create/select a REST app under the client's business account.
2. Copy that app's **sandbox** Client ID and Secret into the first two credential fields. The Secret stays in the backend; it is never sent to the browser.
3. Restart the backend after editing `.env`. Sign into the store, add an item and open checkout. Pay using a separate sandbox buyer account, not the seller's real login. Sandbox payments do not move real money.
4. For testing through a tunnel, set `PAYPAL_RETURN_BASE_URL` to the current public **frontend** HTTPS origin. It must route `/api` to this backend. A localhost return URL only works on your own computer. Update the setting whenever a temporary tunnel URL changes.
5. In the same PayPal app, add a webhook with URL `https://YOUR-PUBLIC-ORIGIN/api/paypal/webhook` and event `PAYMENT.CAPTURE.COMPLETED`. Copy its webhook ID into `PAYPAL_WEBHOOK_ID`. Webhooks require a reachable public HTTPS endpoint; the local callback can be tested without a webhook.
6. `PAYPAL_MERCHANT_ID` is the seller's PayPal merchant account ID, not their email or app Client ID. When populated, captured payments must match it.

## Before accepting real payments

Switch to `PAYPAL_ENVIRONMENT=live` only after sandbox testing. Use the live app's Client ID, Secret, webhook ID and the seller merchant ID. Live mode requires all four and an HTTPS return origin. Credentials and PayPal account verification must belong to the client's business. The API host is selected internally, not supplied by a browser.

Current totals use the existing product/variant prices and quantity, or the accepted custom quote, in USD. **Separate shipping, tax, customs and currency conversion calculations are not implemented.** Confirm your pricing/shipping/tax policy before going live. Payment processing fees are charged by PayPal under the merchant account's terms; the application does not add a surcharge.

This change implements checkout, not automatic PayPal refunds. Existing live refund actions remain blocked. Admin order cancellation does not send money back. Handle real refunds in PayPal until refund integration is added; external refunds/disputes are not automatically synchronized to local order status.

## Payment recovery

- An attempt is saved before contacting PayPal. One active checkout per customer prevents duplicate concurrent attempts.
- The server rechecks prices when creating checkout, freezes its contents and delivery address, and uses persistent idempotency keys for create/capture calls.
- Only a completed capture with the expected order, reference, amount, currency and configured merchant becomes a paid order.
- Callbacks, check-status requests and verified webhooks reconcile the same saved attempt. Row locks serialize concurrent processing.
- If the browser is closed, opening checkout restores the pending attempt. A lost capture response can be recovered by checking PayPal's order.
- Unapproved cancellations keep the bag. Pending captures remain pending. If an attempt cannot be reconciled automatically, retain its reference and check PayPal before taking another payment.
- Sandbox orders are labeled `SandboxPaid` and excluded from verified-purchase reviews, like other test orders.

## Database / validation

Migration `AddPayPalAttempts` adds a dedicated attempts table. Apply with:

```powershell
cd KsSquare-Backend
dotnet ef database update --project KsSquare.Persistence --startup-project KsSquare.Api
```

Automated checks include capture validation, configuration guards, PostgreSQL ownership, price tampering, duplicate retries, lost-response recovery, cancellation, and webhook rejection/idempotency. A real end-to-end sandbox transaction still requires your credentials.

Official integration reference: https://developer.paypal.com/docs/api/orders/v2/
