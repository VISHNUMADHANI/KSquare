# Cart and orders

## Use the feature

1. Open a product, choose options and any personalized name, then **Add to bag**.
2. Open the bag icon or `/cart`. Adjust quantities or remove items.
3. Continue to `/checkout` and sign in as a customer. Guest cart selections carry across sign-in.
4. Enter a full delivery address. The server checks current prices and availability.
5. Choose **Try PayPal demo**, then simulate approval, decline or cancellation.
6. Approval saves a PostgreSQL order and opens confirmation. The customer can follow it at `/orders` or **My orders** in their profile.
7. Admins can view `/admin/orders`, filter by status, inspect order details and update progress.

Priced custom requests use **Checkout quote**. The customer must sign in with the verified email used for that request. A quote can produce only one order.

## Current payment scope

Payments are development-only simulations. Every saved order is explicitly marked `IsDemo=true`, `PaymentStatus=DemoPaid`, with a `DEMO-` payment reference. No money moves, PayPal credentials are not used, and delivery/tax charges are not calculated. `/checkout/demo` redirects to the new cart checkout. The previous standalone sample simulator was replaced by this order-backed flow.

Both the Angular checkout route and the backend demo payment endpoint are disabled in production. Cart and order history are still available. A live PayPal integration requires backend create/capture verification, verified webhooks, idempotency, shipping/tax calculation and real payment state transitions; adding credentials alone is not enough.

## Persistence and access

- Carts use local browser storage, separated by account, with guest cart handoff at sign-in. They do not synchronize across devices. Quantities are limited to 20 per product and 30 distinct selections.
- Orders persist in PostgreSQL. Prices, item names/options, delivery address and customer details are snapshots.
- Customers can list/read only their own orders. Only admins can update status.
- Flow: Confirmed -> Processing -> Shipped -> Delivered. Confirmed or Processing can be cancelled. Delivered and Cancelled are terminal.
- Demo cancellation changes order status only; it does not initiate a real refund.
- A customer-scoped checkout UUID plus request hash prevents duplicate orders, including concurrent requests. A pending confirmation is retained in session storage for safe retry after refresh or a lost response.
- Purchased quantities are removed only after the backend returns the saved order. Decline/cancel leaves the cart unchanged.
- The backend validates product IDs, options, variant availability, personalization, quantities and prices. Browser price estimates cannot set the order amount.
- Status versions reject conflicting admin updates.

## API

- POST `/api/orders/preview`: customer-only server pricing, no write.
- POST `/api/orders/demo`: customer-only, Development only; persist a demo-approved order.
- GET `/api/orders`, GET `/api/orders/{id}`: customer history/details.
- GET `/api/admin/orders`, GET `/api/admin/orders/{id}`: admin history/details.
- PUT `/api/admin/orders/{id}/status`: admin status update with expected version.
- Lists support `status` and `page`, 20 orders per page.

Migration `20260910030017_CustomerOrders` creates only `customer_orders` and its indexes. It has been applied to the local development database. Apply this migration when setting up another development database.

## Verification

- Backend build and frontend production build passed.
- 15 focused xUnit tests passed: pricing, options, unavailable variants, personalization, quantities and status transitions.
- 19 PostgreSQL integration checks passed in an isolated temporary schema that was removed afterward: persistence, historical snapshots, tampered prices, ownership, idempotency (including concurrent submissions), custom quote ownership/uniqueness, and stale status versions.
- 52 browser checks passed using isolated API fixtures: cart persistence/quantity/removal, login handoff, delivery validation, payment outcomes, lost-response retry across refresh, customer/admin orders, status changes, filtering, errors/retry and layouts at 320/390/768/1366px.
- Anonymous requests to customer and admin order APIs return 401.

Focused tests: `dotnet test KsSquare.Storage.Tests --filter FullyQualifiedName~OrderTests` from the backend.
Integration harness: `dotnet run --project ../.orders-work/integration/OrderIntegration.csproj` from the backend; uses its existing environment settings without printing credentials and creates/removes only a unique test schema.
Browser harness: `node .orders-work/browser.cjs` from the workspace, using the running frontend and a dedicated Chrome CDP endpoint on port 9222. Reports/screenshots live in `.orders-work`.

Final verification: 10 additional browser checks passed for the admin checkout redirect, complete default product options, populated cart/checkout layouts, and sidebar fit. Final frontend production build passed without warnings; frontend and backend both returned HTTP 200.

