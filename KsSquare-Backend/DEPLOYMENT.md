# Demo hosting: Vercel frontend and Render backend

## Render backend

Create a Web Service from VISHNUMADHANI/KSquare. Choose Docker, root directory `KsSquare-Backend`, Dockerfile `Dockerfile`, health check `/health`, and the Free instance for a demo. The container listens on port 8080. Use a region close to Supabase Singapore.

Enter the current backend `.env` values in Render Environment (never commit or upload the file to Git). Required groups: DATABASE_*, AUTH_OTP_PEPPER, ADMIN_SETUP_KEY, R2_*, OTP_EMAIL_PROVIDER=resend, RESEND_API_KEY, OTP_EMAIL_FROM. Add PayPal/Etsy settings if using those integrations. Do not use the development OTP sender in Production. Set FRONTEND_ORIGIN to the final Vercel HTTPS origin. Keep payment credentials in sandbox for the demo.

The existing Supabase database already contains the imported schema and data; do not restore it again. Save the deployed backend origin, such as https://your-service.onrender.com. Check `/health` and `/api/products?pageSize=1` before deploying the frontend.

## Vercel frontend

Import the same repository. Root directory: `KsSquare-Frontend`. Framework preset: Other (this project supplies Build Output API files). Install command: `npm ci`. Build command is supplied by vercel.json. Leave output directory override disabled. Add BACKEND_ORIGIN with the Render HTTPS origin, with no path. No database or R2 secrets belong in Vercel frontend environment settings.

The build emits `.vercel/output`: static Angular files, a same-origin `/api/*` proxy to the backend, and a fallback to index.html for Angular routes. This preserves the existing relative API URLs and same-site login cookie behavior.

After deployment, set Render FRONTEND_ORIGIN and PAYPAL_RETURN_BASE_URL to the Vercel origin and redeploy Render. Test catalog, deep links, sign-in, sign-out, admin access, and a sandbox order. Check Resend domain verification before testing email delivery.

## Limits and verification

Render Free sleeps after inactivity and has ephemeral storage. Session encryption keys are currently ephemeral: users may need to sign in again after restart/redeploy. Media remains in R2 and records in Supabase. This is a demo configuration, not a production availability guarantee.

Local preparation restores the missing EmailOtp entity from its last intact revision and raises the Angular component style error budget to 10kb (keeping the 4kb warning). Docker must still be built by Render; local dotnet publish does not verify the Linux image. Publishing requires access to your Vercel and Render accounts.