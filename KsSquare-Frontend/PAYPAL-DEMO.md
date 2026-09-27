# PayPal demo

The standalone payment preview has been upgraded to cart checkout and saved demo orders.

Open `/shop`, choose a product and **Add to bag**, then continue through `/cart` to `/checkout`. Sign in as a customer and enter a delivery address. Successful simulated approval creates a saved order visible under customer **My orders** and admin **Orders**. Decline or cancellation keeps the cart intact.

This remains a local payment simulator, not PayPal Sandbox or a live PayPal integration. No real money is charged. Orders carry **Demo paid** labels. Demo checkout is blocked in production.

See [ORDERS.md](ORDERS.md) for behavior, API contracts, persistence, migration and validation details. The earlier `.paypal-demo` browser scripts target the retired standalone preview; current checks are in `.orders-work`.
