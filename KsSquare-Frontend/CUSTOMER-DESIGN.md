# Customer portal redesign

The customer portal uses the original K Square gold artwork, black brand surfaces, pearl-white content areas, and restrained gold and silver-neutral accents. The supplied symbol and lettering have not been redrawn, recoloured, or replaced. CSS display frames remove empty margins around the existing PNG artwork without modifying the files.

## Customer experience

- Replaced the demo homepage with an editorial introduction, working collection links, live latest products, and a three-step introduction to custom jewellery.
- Applied a consistent palette, readable typography, larger controls, responsive product grids, and clear loading, empty, and error states across the customer routes.
- Restored the original logo in the header, footer, account entry pages, and custom-design section.
- Added dedicated ring, pendant, and earring collection routes; ring matching excludes earrings.
- Fixed catalog retry, retained price filters through refresh, and added inline price-range validation.
- Added keyboard focus containment, Escape dismissal, focus restoration, and scroll locking to customer dialogs. Closed drawers are inert.
- Kept actual catalog prices and option pricing; no sample products, reviews, certifications, or delivery claims were added to the live application.
- Replaced internal development and administrator copy with customer-facing wording.

## Verification

`npm.cmd run build` passes with no Angular warnings or errors. Initial JavaScript and CSS total approximately 334 kB, with estimated transfer size 89 kB (images and fonts excluded).

83 browser assertions passed across 16 routes at widths 320, 390, 768, and 1440 pixels. Checks cover overflow, broken images, runtime exceptions, menu focus, dialog dismissal, filter persistence and retry, category matching, option pricing, and required form fields.

The backend was offline during review. Data-dependent browser checks intercepted API requests using temporary local fixtures; these fixtures are outside the application and do not write backend data. Live email delivery and online payments remain existing backend limitations and were not implemented as part of this design work.

Local preview: http://127.0.0.1:4200/

## Original logo files

- `src/assets/brand/ksquare-wordmark.png` — SHA-256 `3A7FCA43A321E9D4C5F3BECB9306B7DD84F40258AD0CE70847450055C5C0E41F`
- `src/assets/brand/ksquare-symbol.png` — SHA-256 `EF71BF5FEE773E609D942EBB7C078712B40F29BBD568D2A0451277999EEDE32E`

## Editorial image provenance

The photography is an editorial illustration of the brand aesthetic, not an inventory photograph. Original logo artwork was not submitted to image generation.

Final asset: `src/assets/editorial/jewellery-atelier-black.png`, generated using the built-in imagegen tool.

Initial asset, retained as an unused earlier version: `src/assets/editorial/jewellery-atelier.png`.

Initial prompt:

> Use case: ads-marketing. Asset type: luxury jewellery website editorial hero photograph, landscape 1536x1024. Create a photorealistic high-end jewellery still life: a substantial polished yellow-gold chain necklace draped naturally over a warm ivory travertine sculptural block, with a small elegant gold ring on the lower stone ledge. Background deep forest green textile with soft folds and rich shadows. Close-up product photography, warm directional late-afternoon studio light from upper left, delicate realistic reflections and intricate chain link detail, refined restrained composition, tactile stone and fabric, fine-grained editorial finish. Jewellery occupies the central and right two thirds with generous dark-green breathing room. Luxurious but natural, not oversaturated, not glittery. No text, logo, watermark, boxes, or people. This is editorial brand imagery, not an actual inventory product.

Final edit prompt:

> Edit this editorial jewellery photograph for a black, gold, silver and pearl-white luxury website. Change ONLY the deep green fabric backdrop to neutral charcoal-black fabric with soft silvery highlights. Preserve the exact gold chain necklace and gold ring, stone sculpture, object placement, camera framing, shadows and warm metal reflections. Remove all green colour casts from fabric. Retain beautiful natural rich texture and photographic realism. Do not add text, logos, watermark, or additional objects. This is a photographic background palette adjustment; no logo is involved.

## Compact customer layout — September 9, 2026

Reduced the main customer header from 136 px to 84 px on desktop and from 112 px to 72 px on smaller screens, plus a slimmer announcement strip. The original logo is scaled proportionally within its smaller display frame. Homepage hero photography, headline, supporting text, collection icons/cards and section spacing are smaller.

The profile page now places View my custom requests, Change password and Sign out in a compact action row. Personal information uses a shorter card with three fields across on desktop. Removed the large Password & access panel. Change password opens the existing accessible popup and uses the existing password-change service. The custom-request action links directly to `/custom/requests`, preserving the existing saved-request tracking. No orders UI or order functionality was added.

Production build passes without warnings or errors. Browser checks use temporary data in an isolated browser context and do not change real profiles or passwords. Verification results and screenshots are saved in `../.customer-compact/`.

Final verification: 116 browser assertions passed across 16 customer routes and widths 320, 390, 768, 1366, 1440 and 1920 px. Checks cover header/logo sizing, hero photo and card dimensions, profile layout, popup focus/dismissal, password validation and submission, and navigation to a saved custom request. No browser runtime errors were reported.

## Homepage hero refinement — September 9, 2026

Reworked the homepage introduction into an inset ivory panel with a framed jewellery photograph, a shorter two-line headline, clear collection/custom-design actions, and a compact image caption. Collection cards now share subtle rounded borders. Customer header artwork is smaller again, with a 72 px desktop header and 68 px mobile header (excluding the announcement strip and border). The original logo and editorial photograph files are unchanged.

The project is running at `http://localhost:4200/`, with its backend at `http://localhost:5000/`. The backend was restarted with access to its Windows-protected authentication keys. The production build passes without warnings or errors. Latest preview screenshots and isolated browser verification are in `../.home-polish/`.
