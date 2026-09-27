# Pendant pricing and return review

The pendant collection recognizes Pendant/Pendants and existing Pendate/Pendates and Pandate/Pandates category names. Existing category records are preserved.

In Add/Edit product, select only offered pendant sizes and enter a price for each. The starting price is derived from the configured sizes; customers see the selected size price. With no sizes selected, enter one original price and the customer size selector is hidden. Existing products without size pricing retain their previous base price until edited.

Admin > Returns provides status filters and review dialogs for approval, decline and inspection/refund. Refund amounts are chosen by the administrator within the paid item amount. Refunds remain demo simulations; no real PayPal refund is issued. Existing custom/name-personalized exclusions remain enforced.

Customers can open Returns & refunds in the footer to read the return, exchange and refund policy.

Validation: frontend production build, backend build, 25 isolated PostgreSQL integration checks and 38 mocked browser checks including 320/390/768/1366px layouts. The PendantSizePricing migration adds the size-price table without rewriting existing product data.
