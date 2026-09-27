# Admin portal redesign

Completed September 8, 2026.

The admin portal now uses a charcoal sidebar, pearl-white work areas, restrained gold accents, the original K Square artwork, larger controls and responsive layouts.

## Pages

- `/admin`: new overview showing current total products, active products, top-level categories and submitted requests. Collection bars use actual product counts. Recent requests open their matching detail dialog. Individual failed data sources show unavailable totals rather than zero.
- `/admin/products`: responsive product cards, readable filter controls, explicit edit buttons and a redesigned editor for product information, prices, combinations and images.
- `/admin/categories`: category and subcategory tables with keyboard-accessible selection and clear actions.
- `/admin/product-options`: organized size and color groups with consistent editing dialogs.
- `/admin/custom-requests`: status filters including Approved, request cards, customer details and the existing contact/status/quote workflow.

Navigation and dialogs contain keyboard focus, support Escape and restore focus. Closing a product editor still checks for unsaved changes and is disabled during saving. Dialog validation and API errors remain visible inside the editors. Admin styles are scoped under `.admin-shell`; customer styling and original brand image assets are preserved.

## Verification

- `npm.cmd run build`: passes without warnings or errors. Initial bundle approximately 339 kB; estimated transfer approximately 90 kB, excluding images and fonts.
- `node ../.admin-redesign/verify.cjs` from the frontend directory, or `node .admin-redesign/verify.cjs` from the workspace root: 104 browser assertions passed with no runtime exceptions.
- Five admin routes and four editor dialogs checked at 320, 390, 768 and 1440 pixels for overflow, images and focus behavior.
- Checked overview totals and request links, menu focus and scroll restoration, search and category filtering, product loading and discount updates, category/option validation and creation, contact/quote workflow, unavailable data, retries, empty states and storefront style isolation.

Browser verification used intercepted temporary API fixtures, including an administrator session. No backend data was changed. These checks verify frontend behavior and request payloads; they do not certify live authentication, database persistence, email delivery or payment processing. Fixtures and preview screenshots are outside the application source and build assets. Jewellery imagery in fixture screenshots is editorial illustration, not inventory data.

Local portal: http://127.0.0.1:4200/admin (requires an existing administrator login).

Desktop preview: `../.admin-redesign/preview-admin-1440.png`
Mobile preview: `../.admin-redesign/preview-admin-390.png`
Detailed browser report: `../.admin-redesign/verification.json`

## Compact layout update — September 9, 2026

Reduced the desktop sidebar from 240 px to 204 px, scaled the original logo proportionally, removed the decorative sidebar message, and tightened navigation and footer spacing. Smaller headings, page padding, dashboard sections and product cards make the main workspace more compact. Controls retain larger touch targets on regular mobile screens, with additional spacing adjustments for short landscape screens.

The sidebar keeps spare vertical room for three future navigation entries. No Orders, Payments, placeholder links, dashboard metrics or new features were added. The sidebar content fits its viewport rather than hiding overflowing controls.

Production build passes without warnings or errors. Responsive verification details are saved in `../.admin-compact/verification.json`; preview images are in `../.admin-compact/`.
