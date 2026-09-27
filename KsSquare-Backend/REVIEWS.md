# Product reviews

Customers with a delivered order can submit one review per product only from the Write a review button beside an item in My orders. A dedicated popup opens without leaving the order. Product pages display published reviews only. The API requires the selected order ID and checks ownership, Delivered status and membership of the product in that exact order. A review has 1-5 stars, the account first name (assigned by the API), text, and publication consent. Demo orders never receive a verified purchase badge.

Media limits: four JPG/PNG/WebP photos at 5 MB each, plus one MP4/WebM/MOV video at 25 MB. Both the browser and API validate limits; the API checks file signatures. Uploads remain in the existing private R2 bucket. Signed media URLs last one hour. Original video files are played using browser controls; there is no video transcoding, so playback depends on the uploaded codec. MOV camera videos are accepted when they carry an ftyp header; HEIC photos are not accepted, so use JPG instead. The review form offers only Choose files for selecting existing photos and videos. Files are previewed before submission.

Admin > Reviews lists pending, published and rejected submissions. Rejection/unpublishing requires a note visible to the submitter. Only published reviews contribute to public ratings or media. Publish is protected by the Admin role and optimistic concurrency. Customer/order identifiers and moderation notes are not exposed by the public list. Apply the same moderation standard regardless of rating.

Migration: ProductReviews adds product_reviews, its indexes and foreign keys. Run dotnet ef database update --project KsSquare.Persistence --startup-project KsSquare.Api from KsSquare-Backend for other environments.

API routes:
- GET /api/reviews/product/{productId}?page=1&sort=newest&mediaOnly=false
- GET /api/reviews/product/{productId}/eligibility?orderId={orderId} (Customer)
- POST /api/reviews/product/{productId} (Customer; multipart including orderId)
- GET /api/reviews/admin?status=Pending&page=1 (Admin)
- PUT /api/reviews/admin/{id} (Admin; status, note, version)

The homepage presents eight available products first, followed by editorial/custom sections. Store currency remains USD. No shipping or delivery promises have been added.

Public product review lists, averages, rating breakdowns and media filters are shared across products with the same primary CategoryId. Each review includes the name of the purchased product. Writing remains tied to the exact delivered product and order; category sharing does not grant review eligibility for other products.

My orders checks GET /api/reviews/order/{orderId}/submitted before displaying review actions. Reviewed products show View your review, including after reload. The eligibility response includes the authenticated customer's existing review (including pending/rejected status, moderation note and private media) for read-only viewing. Submitted lookup checks order ownership; public lists still include only published reviews.
