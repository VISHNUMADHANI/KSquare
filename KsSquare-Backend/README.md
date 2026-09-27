# KsSquare Backend

## Cloudflare R2 storage

Copy `.env.example` to `.env` and supply the R2 values locally. Never place R2 credentials in Angular, source code, `appsettings.json`, or version control.

- Product media is public and stored in `R2_PUBLIC_BUCKET_NAME`.
- Customer custom-request media is private and stored in `R2_PRIVATE_BUCKET_NAME`.
- Accepted content types are JPEG, PNG, and WebP; each file is limited to 10 MB.
- A custom request may contain at most five files; its feature will enforce that aggregate rule.
- Store object keys and metadata in PostgreSQL, never image bytes.
- Read private files using short-lived signed URLs.

Key conventions are `products/{productId}/{variant}/{uuid}.{extension}` and `custom-requests/{requestId}/{uuid}.{extension}`. The `r2.dev` URL is for development; configure a custom domain before production.
