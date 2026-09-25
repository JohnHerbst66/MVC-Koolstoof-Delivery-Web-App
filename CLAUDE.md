# Koolstoof MVC WebApp

Shared project context for Claude Code (CLI) and Claude Cowork. Both tools read this file automatically — update it whenever a decision, gotcha, or piece of in-progress work is worth the other one knowing about. Keep entries short; put deep detail in code comments or commit messages instead.

## Project overview

ASP.NET Core MVC web app, project name `Koolstoof-App-1`, root namespace `Koolstoof_App_1`.

- Target framework: **.NET 10** (`net10.0`)
- Nullable reference types + implicit usings: enabled
- Auth: **ASP.NET Core Identity** (`AddDefaultIdentity<IdentityUser>`, `RequireConfirmedAccount = true`), scaffolded under `Areas/Identity`
- Data access: **Entity Framework Core** with **SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer`)
  - `Data/ApplicationDbContext.cs` — inherits `IdentityDbContext`, currently no custom `DbSet`s added yet
  - Migrations live in `Data/Migrations`
- MVC pattern: standard `Controllers/`, `Models/`, `Views/` folders
- Docker: has a `Dockerfile` + `.dockerignore`, targets Linux containers (VS container tooling included)
- Repo: **GitHub** — https://github.com/JohnHerbst66/MVC-Koolstoof-Delivery-Web-App, `main` branch, pushed 2026-08-13

## Folder layout

```
Koolstoof-App-1/
  Areas/Identity/       Identity UI scaffolding
  Controllers/          HomeController.cs so far
  Data/                 ApplicationDbContext + Migrations
  Models/               ErrorViewModel.cs so far (app models not yet added)
  Views/
  wwwroot/
  Program.cs             app startup/config
  Koolstoof-App-1.csproj
```

## Working conventions

- The user (John) wants full read/write access — Claude can view and modify any file in this project, and is expected to debug issues directly rather than just describing them.
- When something breaks, investigate root cause before proposing a fix (this project also uses the `engineering:debug` skill in Cowork).
- Connection string is read from config as `DefaultConnection` (see `appsettings.json` / `appsettings.Development.json` / user secrets) — don't hardcode credentials in code or commit them.
- **Claude's role split (set 2026-08-13):** Claude acts as (1) John's assistant for docs and planning — keep CLAUDE.md and Notes.md updated as decisions are made, produce diagrams/plans as asked — and (2) John's teacher. **John will do most of the actual coding himself.** When implementing features, Claude should teach: explain *how* to build something and, just as importantly, *why* it's built that way (trade-offs, patterns, gotchas) — not just write the code and hand it over. Default to walking John through the approach and letting him drive the typing, unless he asks Claude to just write the code directly.

## App design — customer-facing page flow (planned)

Home → Menu → Cart → Checkout → Payment → Order Confirmation/Status.

- **Home**: specials/featured items, open/closed status (delivery hours), "Order Now" CTA. Also serves as the marketing page.
- **Menu**: categorized/browsable (menu is large), items added to a persistent mini-cart shown in the header. Specials highlighted inline where they appear.
- **Cart**: review/edit items and quantities.
- **Checkout**: distinct step from Cart — guest checkout by default (optional account to save addresses/history), delivery address restricted to a fixed Thabazimbi delivery-area list, delivery fee shown, delivery-hours check happens here (before payment, not after).
- **Payment**: redirect to payment gateway (PayFast likely, TBD), webhook/ITN confirms payment server-side — order only marked Paid on webhook confirmation, not just browser redirect.
- **Order Confirmation/Status**: immediate confirmation after payment, then ongoing status tracking (Received → Preparing → Out for Delivery → Delivered).

**Home page layout is fixed/"set in stone"** — not a free-form page editor. Staff cannot rearrange the page; they can only fill in content for predetermined slots: banner/hero image(s) (uploaded), specials (pulled live from `MenuItem.IsSpecial`, no separate editing), announcement message (short text + active on/off toggle, e.g. for holidays/loadshedding), ordering hours (read from the same settings used to enforce checkout cutoff — one source of truth shown in two places). This keeps the admin-editable surface small and safe for non-technical staff.

## App design — admin-facing page flow (planned)

Staff log in and land on the **Orders dashboard** by default (highest-frequency task — replaces answering the phone). Persistent admin nav: Orders / Menu / Home content, plus account/logout. Order count badge on the Orders nav link.

- **Orders dashboard (default landing page)**: active orders (Paid/Preparing/Out for Delivery) shown first; older/completed orders in a separate filter/tab. Each row shows items, delivery address, payment status, time pending, with one-click status-advance buttons (Preparing → Out for Delivery → Delivered) rather than a full edit form.
- **Menu editor**: list-first, grouped by category, mirrors the customer-facing menu structure. In/out-of-stock toggle inline on the list (most frequent action). Click an item to open the full edit form (name, description, price, image, `IsSpecial`, category). Prominent "Add new item." Lightweight category management (add/rename/reorder) in the same area.
- **Home content editor**: short fixed form — upload banner image(s), write/toggle the current announcement. Hours and specials are *not* edited here (they live in Settings and Menu respectively, per the fixed-layout note above).

Staff accounts: individual logins per staff member (not one shared login), all under the Admin role — supports accountability (who changed what) if that's added later.

## Hosting — Vercel + Neon Postgres (decided 2026-09-25)

Live at **https://koolstoof.vercel.app** (Vercel project `koolstoof`, team `john-1cbd`), deployed from the GitHub repo's `main` branch. Chosen over Azure because John wants no monthly costs. (History: SQL Server + Azure App Service/Blob Storage was the original plan from 2026-08-13; dropped.)

- **Runtime**: Vercel's container runtime — `Dockerfile.vercel` + `vercel.json`. The container is **stateless and scales to zero**, has no persistent disk, and requests are capped at 4.5 MB. `.vercelignore` keeps local `bin/`/`obj/` out of the upload.
- **Database**: **Neon Postgres** (Vercel Marketplace, resource `neon-cobalt-forest`), one relational DB for everything. The app uses the *unpooled* URL, passed as `ConnectionStrings__DefaultConnection` (`postgres://` URLs are converted in `Program.cs`).
- **Because it is stateless**: the cart is an encrypted cookie (`Services/CartStore.cs`), Data Protection keys are stored in the DB (logins survive restarts), and uploaded photos are stored in the DB (`StoredImage`, served at `/media/{id}`, resized in the browser before upload to stay under the size cap).
- **Time**: the server runs in UTC; `Helpers/SouthAfricaTime` gives South African time (fixed UTC+2). Times are stored in `timestamp without time zone` columns.
- **Config / secrets** are Vercel environment variables (Production), never committed: `ConnectionStrings__DefaultConnection`, `PayFast__MerchantId/MerchantKey/Passphrase`, `WhatsAppCloud__PhoneNumberId/AccessToken`. Locally they live in .NET user-secrets. Admin passwords are only generated in Development (written to the gitignored `admin-credentials-CONFIDENTIAL.txt`); in production, accounts need `AdminSeed__Passwords__IDxxx` set or they are skipped.
- **Deploying**: pushing to `main` deploys to production automatically; `vercel deploy --prod` also works. Public address `koolstoof.vercel.app` is open to customers; the per-deployment URLs sit behind Vercel login.
- **PayFast** is on sandbox credentials. The ITN (`/Checkout/PayFastNotify`) signature covers *every* posted field including blanks (unlike the outgoing payment signature), and the paid amount must match the order.
- **WhatsApp alerts** use an approved Meta template (`new_order_alert`). The token must be a permanent System User token — a temporary token expires after 24 hours and silently stops alerts.

## Orders — hiding and deleting (2026-09-25)

Admin orders board (`/Order/Manage`). Rules live in `Order.CanBeDeleted` and are enforced server-side:
- **Any order can be hidden** (`ArchivedAt` set); hidden orders appear under Hidden orders and can be restored. "Clear delivered" hides all delivered orders at once.
- **Paid orders can never be permanently deleted** — hide only. Cash orders count as paid once marked Delivered.
- Only never-paid orders can be deleted, and a PayFast order that isn't paid yet is protected for 24 hours (its payment confirmation may still arrive).
- Every action goes through a confirmation page; deleting also requires typing the order number.

## Cross-Claude Q&A

Async bridge between Claude Code (Code guy) and Claude Cowork (Docs guy) — John relays messages between the two chats. Ask a question by appending it here under **Open questions**; when answered, move it to **Answered** with the answer inline and date.

### Open questions

_(none open — see Answered below)_

### Answered

- **[2026-08-13, from Code guy, answered by Docs guy]** Q: Can you put together a database diagram (ERD) for the schema? A: Done. Entities: `ApplicationUser` (Identity, roles Admin/Customer), `MenuCategory` → `MenuItem` (with `IsInStock`, `IsSpecial`/`SpecialPrice`, `ImageUrl`), `DeliveryArea` (Thabazimbi suburb list + optional fee), `CustomerAddress` (optional, tied to a `DeliveryArea`), `Order` (nullable `UserId` for guest checkout, guest name/phone/address captured directly, `DeliveryAreaId`, `DeliveryFee`, `Status`) → `OrderItem` (snapshots item name/price at order time), `Payment` (gateway reference, confirmed only via webhook — not browser redirect), `Announcement` and `RestaurantSettings` (backing the fixed home-page slots: banner image, announcement text, hours). Full Mermaid ER diagram + a matching flow diagram (customer + admin) + DFD 0/1 were built as Cowork artifacts — ask John to pull them up in the Cowork sidebar ("koolstoof-db-schema", "koolstoof-app-flow", "koolstoof-dfd"); can paste the raw Mermaid source into this file instead if Code guy needs it inline — just ask via a new Open question.

## Notes.md

There's a `Notes.md` file in the project root for John's loose ideas and things he wants to revisit later but doesn't want to act on right now. When John says "put it in Notes" (or similar), append the idea to `Notes.md`, dated, without deleting existing entries.

## Status / open items

- App is early-stage: only the default Home controller/views and Identity scaffolding exist so far. No custom domain models, DbSets, or business logic added yet.
- 2026-08-13: Repo initialized and pushed to GitHub (`main` branch). Added `.gitignore` (excludes build output, secrets, and Docker Desktop's unrelated `welcome-to-docker` sample folder that had been sitting in the project directory).
- _(Add new entries below as work progresses — date them.)_
