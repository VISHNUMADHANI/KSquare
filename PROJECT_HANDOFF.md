# K Square Project Handoff

This file summarizes the current K Square Gems & Jewellery project work so another developer or AI can understand the project without reading the full Codex chat history.

## Project

K Square is a jewelry storefront and admin portal with:

- Angular frontend in `KsSquare-Frontend`
- .NET 8 backend in `KsSquare-Backend`
- PostgreSQL persistence
- Cloudflare quick tunnels used for temporary preview links
- R2/private media storage for customer and product uploads

The store is aimed at customers outside India, with a black/gold luxury jewelry style inspired partly by Kloira-style ecommerce pages.

## Current Main Areas

### Storefront

- Home page has been redesigned around a premium black/gold jewelry look.
- Main navigation focuses on custom orders and core categories.
- Logo click returns to home.
- Home includes:
  - Custom pendant hero
  - Category cards
  - Latest products
  - Etsy review trust header
  - Customer review/media sections
- Product detail pages support:
  - Photos and videos
  - Color/size options
  - Name pendant personalization
  - Optional customer instructions
  - Product details and returns in popups
  - Category-level review/media sections

### Admin

- Admin portal has pages for:
  - Overview/revenue dashboard
  - Orders
  - Payments
  - Customers
  - Reviews
  - Returns
  - Products
  - Categories
  - Sizes and colors
  - Custom requests
- Product cards are compact, image-first, and clicking a card opens edit directly.
- Product duplicate flow was added:
  - Duplicate button appears on product cards.
  - It opens a prefilled product form.
  - Add button stays disabled until the duplicate has at least one change.
  - If changes are reverted, Add disables again.
  - Duplicate copies photos/videos only when saving, so opening the form is faster.
  - New duplicated product gets its own media copies and does not touch the original.

### Reviews

- Customers can review only delivered order items.
- Once a user has reviewed an item, they should not be prompted to review it again.
- Reviews can include photo/video uploads from files only.
- Review media deletion UI uses hover/delete-icon behavior.
- Reviews are shared by category. For example, bracelet reviews can show across bracelet products.
- Category reviews show even if the original reviewed product is unavailable.
- Admin can manually add imported category reviews with customer name, rating, text, and photos.
- Product pages include a customer media section inspired by Kloira:
  - Circle/story style media previews
  - Clicking opens a larger media/review dialog
  - Photo/video reviews are emphasized for trust

### Etsy Reviews

- Etsy review integration was explored.
- Env placeholders exist for Etsy credentials.
- Reviews are cached to avoid Etsy API rate limits.
- UI shows Etsy average and total review count as trust proof.
- Public UI should not link out to Etsy unless explicitly changed later.
- Etsy credentials and shop details must stay in environment variables, not source.

### PayPal

- PayPal integration was started.
- Checkout shows PayPal availability based on env credentials.
- Env placeholders exist for PayPal client id/secret/webhook id.
- Demo/test cards are not enough for live payment. Real PayPal app credentials are needed.

### OTP Email

- OTP email was switched toward Resend.
- Backend supports:
  - `OTP_EMAIL_PROVIDER=resend`
  - `RESEND_API_KEY`
  - `OTP_EMAIL_FROM`
- For testing without a verified domain, Resend can use:
  - `K Square <onboarding@resend.dev>`
  - It only sends to the Resend account owner email.
- For real customers, the domain must be verified in Resend.
- Current code has a Resend sender and a safe delivery error message.

## Custom Jewelry

Custom order flow is a key selling point.

Current direction:

- Customers choose custom product type, style, size, color, and comments.
- Pendant and pendant+chain/set style flows should support different images for different choices.
- Name pendant pricing changed:
  - Admin enters original/base price.
  - Admin enters included letter count.
  - Admin enters price per extra letter.
  - Customer max letter limit is 8.
  - Price increases only for letters above the included count.
  - Discount percentage is shown on the total/final price.
- Grillz/custom color options should show where relevant.

## Product Media

- Admin product listing supports photos and videos.
- Product media limit is 6 total photos/videos.
- Videos should be supported in admin and customer product detail.
- Duplicate product form copies media lazily on save instead of loading everything immediately.

## Admin Dashboard

Overview dashboard includes:

- Total revenue
- Total orders
- Customers
- Pending payments
- Recent orders
- Payment/status summaries
- Filters:
  - All time
  - Today
  - Weekly
  - Monthly
  - Yearly
  - Custom date range

Revenue excludes demo/test orders where implemented.

## SEO

SEO was discussed but not fully implemented.

Recommended approach:

- Add editable SEO fields for products/categories:
  - Meta title
  - Meta description
  - Slug/canonical
  - Open Graph image
- Use Angular title/meta services for client metadata.
- Consider SSR/prerender later for stronger search indexing.
- Generate sitemap and robots.txt for production.

## Running Locally

Frontend usually runs on:

```text
http://localhost:4200
```

Backend usually runs on:

```text
http://localhost:5000
```

Cloudflare quick tunnel links are temporary. If a link stops working, start a new tunnel from `.local-run/cloudflared.exe` against `http://localhost:4200`.

## Environment Notes

Do not commit real `.env` values.

Important env groups:

- Database connection
- R2 media storage
- PayPal credentials
- Etsy credentials
- Resend OTP email credentials

Backend `.env` and `.env.example` include placeholder keys for newer integrations.

## Git Notes

The GitHub repository used earlier:

```text
https://github.com/VISHNUMADHANI/KSquare
```

Initial project code was pushed earlier. Later local work may still be uncommitted/unpushed, so check `git status` before assuming GitHub is current.

## Good Next Steps

1. Review current `git status`.
2. Run frontend and backend builds.
3. Test duplicate product with a real product that has images/videos.
4. Verify Resend OTP with a test email, then verify the real domain for customer emails.
5. Complete SEO metadata fields and public metadata rendering.
6. Push the latest stable code after checking no secrets are staged.

