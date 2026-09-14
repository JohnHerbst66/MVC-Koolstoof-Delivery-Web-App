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

## Data storage — Azure (decided 2026-08-13)

One relational database for everything — **Azure SQL Database** (managed SQL Server in Azure, PaaS). Holds Identity users/roles, menu categories/items, orders/order items, delivery areas, settings/announcements. Rejected a SQLite (auth/orders) + MongoDB (menu) split: orders need to reference menu items via foreign keys, order placement + payment needs transactional guarantees (all-or-nothing), and the menu's shape is relational, not document-shaped — splitting into two databases would add complexity and risk (e.g. an order recorded without its items) for no benefit. Matches what the project already has configured (`Microsoft.EntityFrameworkCore.SqlServer`, `ApplicationDbContext`) — moving to Azure SQL for production is a connection-string change, not a rewrite.

Images (menu photos, home page banners) go in **Azure Blob Storage**, not the database — only the resulting URL is stored in SQL (e.g. on `MenuItem`, home-content settings). Standard practice; databases handle large binary files poorly.

Likely hosted on **Azure App Service**, keeping everything (DB, storage, hosting) in one cloud provider.

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
