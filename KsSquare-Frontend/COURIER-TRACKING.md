# Courier tracking links

Admin Orders > View order details > Mark shipped now requires USPS, UPS, DHL, FedEx or Aramex plus a tracking number. Details are stored on the order; the API builds HTTPS links from a fixed courier allowlist. Both customer and admin details display Track parcel in a new tab. Shipped orders allow correcting or adding tracking through Edit/Add tracking, including older shipped orders.

Mark delivered requires the actual delivery date/time from the courier, entered in the admin browser local timezone and stored as UTC. The server rejects dates before order creation or in the future. The seven-day return window uses that date. This integration does not buy labels, contact couriers, or automatically update order status.

Existing orders keep null courier/tracking values. No fictitious shipment details were added. One tracking number per order is supported; split shipments would need a separate shipment model.

Validation: both builds, 38 isolated PostgreSQL checks (including all courier mappings, persistence, invalid data and delivery dates), 10 mocked browser checks including customer/admin links and responsive forms. Actual shipment lookup requires valid client tracking numbers; courier websites can redirect, require cookies, or temporarily lack tracking events.
