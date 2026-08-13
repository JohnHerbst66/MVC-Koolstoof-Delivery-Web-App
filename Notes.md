# Notes

Running list of ideas, features, and things to look into later. John adds to this by saying "put it in Notes" — Claude should append below (dated), never delete existing entries without being asked.

---

## 2026-08-12

- **WhatsApp staff notification on new/pending orders** — send staff a WhatsApp message ("You have orders") when an order is placed (marked Paid), and a reminder if an order sits Pending for too long. Plan: use Twilio's WhatsApp Business API (or similar provider) with a pre-approved message template, triggered from the order-paid code path plus a small background service that periodically checks for stale pending orders. Needs a Twilio account + WhatsApp sender/template approval from Meta before it can go live (that approval is the main lead time, not the coding). Slot this in around Stage 3/4 of the build (once payment + order status flow exist).

- **Customer-facing page flow (agreed):** Home (specials/featured items + open/closed status + Order Now CTA) → Menu (categorized, browsable, add to cart, persistent mini-cart in header) → Cart (review/edit quantities) → Checkout (guest or account, delivery address restricted to Thabazimbi, delivery fee shown, delivery-hours check) → Payment (gateway redirect) → Order Confirmation/Status (immediate confirmation, then ongoing status tracking). Checkout is a distinct step from Cart so delivery-area/hours validation happens before payment, not after.

