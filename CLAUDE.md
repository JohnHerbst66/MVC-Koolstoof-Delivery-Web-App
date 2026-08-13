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
- Repo will be pushed to **GitHub**

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

## App design — customer-facing page flow (planned)

Home → Menu → Cart → Checkout → Payment → Order Confirmation/Status.

- **Home**: specials/featured items, open/closed status (delivery hours), "Order Now" CTA. Also serves as the marketing page.
- **Menu**: categorized/browsable (menu is large), items added to a persistent mini-cart shown in the header. Specials highlighted inline where they appear.
- **Cart**: review/edit items and quantities.
- **Checkout**: distinct step from Cart — guest checkout by default (optional account to save addresses/history), delivery address restricted to a fixed Thabazimbi delivery-area list, delivery fee shown, delivery-hours check happens here (before payment, not after).
- **Payment**: redirect to payment gateway (PayFast likely, TBD), webhook/ITN confirms payment server-side — order only marked Paid on webhook confirmation, not just browser redirect.
- **Order Confirmation/Status**: immediate confirmation after payment, then ongoing status tracking (Received → Preparing → Out for Delivery → Delivered).

Admin-side page flow (planned, not yet detailed): Home page content editor, Menu item editor, Orders dashboard. Full admin flow to be worked out next.

## Notes.md

There's a `Notes.md` file in the project root for John's loose ideas and things he wants to revisit later but doesn't want to act on right now. When John says "put it in Notes" (or similar), append the idea to `Notes.md`, dated, without deleting existing entries.

## Status / open items

- App is early-stage: only the default Home controller/views and Identity scaffolding exist so far. No custom domain models, DbSets, or business logic added yet.
- _(Add new entries below as work progresses — date them.)_
