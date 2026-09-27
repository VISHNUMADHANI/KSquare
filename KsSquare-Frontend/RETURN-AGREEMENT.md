# Return agreement flow

Regular products are eligible within the existing seven-day delivery window and condition requirements. Custom quote orders and name-personalized pieces remain excluded. The previous generic final-sale product flag no longer excludes regular items; historical snapshots explicitly marked "This item is final sale." are treated as regular. Older orders without eligibility/delivery information still require manual review.

Admin Returns: approve or decline; use WhatsApp/email links to contact the customer; select Agree refund amount, explain shipping/other deductions, and confirm customer agreement. The amount is saved with the decision history without issuing payment. Customer orders show the agreed amount. Links open contact drafts only; the application does not send messages automatically. WhatsApp needs a customer phone number with country code.

An approved, agreed amount can later be recorded as a demo refund after receipt/inspection. The API requires it to match the saved agreement. Real refunds remain unconnected. No migration is needed; agreement uses the existing return JSON and history.

Validation: backend build, frontend production build, 63 backend tests, and eight browser checks for contact links, required conversation confirmation, agreement display and mobile/desktop layouts.
