# OTP email with Resend
1. Add your sending domain in Resend. Add the provided DNS records at your DNS host and wait for verification. Keep existing mailbox MX records; do not replace them.
2. Create a sending API key restricted to your sending domain.
3. Set these backend .env values:
   OTP_EMAIL_PROVIDER=resend
   RESEND_API_KEY=your_key
   OTP_EMAIL_FROM="K Square <noreply@your-verified-domain>"
4. Restart the backend. Register with a test inbox or request a password reset, then check Resend delivery logs.
The backend sends HTML and plain text emails. Codes expire after 5 minutes; requesting another code requires a 60-second wait. Provider failures return a safe retry message, without logging codes, API keys or provider response bodies.
For local console-only testing, explicitly set OTP_EMAIL_PROVIDER=development. This mode is blocked outside Development. Resend is the default and has no console fallback.
Never commit .env or put RESEND_API_KEY in the Angular app.
Domain verification: https://resend.com/docs/dashboard/domains/introduction
