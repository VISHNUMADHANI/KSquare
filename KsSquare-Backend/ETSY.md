# Etsy shop reviews

Fill these existing placeholders in `KsSquare-Backend/.env` and restart the backend:

```dotenv
ETSY_API_KEY=
ETSY_SHARED_SECRET=
ETSY_SHOP_ID=
```

Use an approved Etsy app's Keystring and Shared Secret from https://www.etsy.com/developers/your-apps. `ETSY_SHOP_ID` is the numeric shop ID, not the shop name or seller login. If you only know the shop URL, provide that URL so its ID can be resolved using Etsy's shop lookup endpoint with your API key. Do not put credentials in frontend files. The public shop review endpoint does not require OAuth.

The homepage and every catalog/category route show the same clearly labelled Etsy shop reviews below their main content. Reviews are not mapped to products/categories or included in this site's product star averages. All ratings are retained, including low ratings. Reviewer identities are labelled Etsy customer because the review endpoint does not supply a display name; private numeric buyer/transaction IDs are not exposed.

Up to 100 reviews from Etsy's first API page are fetched and sorted by review date. A horizontal carousel shows text, ratings and customer photos, without external links. The header displays the full count returned by the reviews endpoint and the official shop average (labelled as the past year rating). The average is not calculated from the 100 displayed cards. Photos are supported when returned by Etsy; review videos are not provided by this endpoint. No sample reviews are published.

Shared backend memory cache refreshes on the first visit after six hours. It retries errors after five minutes, retains previously fetched data for at most seven hours if refreshing fails, and disappears if unavailable or empty. No background worker is needed. This cache resets on restart. Missing credentials and API failures return an empty feed without affecting shopping. The browser reuses the feed while navigating; reload to fetch a fresh snapshot.

Validation covers upstream authentication, shared caching, preserving low ratings, safe media URLs, private-field exclusion, and unavailable API behavior. Actual shop connectivity must be verified after the credentials and numeric shop ID are supplied.

Reference: https://developers.etsy.com/documentation/reference/#operation/getReviewsByShop
