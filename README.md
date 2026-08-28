# Launch Assist Studio — Website

Production website for **Launch Assist Studio** (launchassiststudio.com): websites, custom .NET software, e-commerce and branding.

Built with **.NET 10 / ASP.NET Core Razor Pages**, **Entity Framework Core** and **SQL Server**, faithfully reproducing the approved HTML/CSS design reference.

## Solution layout

```
LaunchAssistStudio.slnx
src/LaunchAssistStudio.Web/
  Pages/            Razor Pages (all public pages + intake form)
  Pages/Seo/        SEO landing pages (/web-design, /dotnet-development, ...)
  Pages/Shared/     _Layout + reusable _CaseStudy component
  Data/             AppDbContext, entities (Lead, LeadNote, LeadStatusHistory), migrations
  Models/           Form input model + whitelisted option lists
  Services/         SMTP email sender (MailKit) + lead email composer
  wwwroot/          css/site.css (ported from design reference), js/site.js, robots.txt, sitemap.xml
```

## Pages

| Route | Page |
|---|---|
| `/` | Home |
| `/services`, `/pricing`, `/portfolio`, `/about`, `/contact` | Main site |
| `/start-project` | Lead intake form (server-validated, saved to SQL Server) |
| `/start-project/thank-you` | Submission confirmation |
| `/web-design`, `/ecommerce-development`, `/custom-software-development`, `/dotnet-development`, `/sql-server-development` | SEO landing pages |

## Running locally

Prerequisites: .NET 10 SDK, SQL Server LocalDB (installed with Visual Studio).

```bash
dotnet run --project src/LaunchAssistStudio.Web
```

On first run the app creates the `LaunchAssistStudio` database on `(localdb)\MSSQLLocalDB` and applies EF Core migrations automatically (`Database:ApplyMigrationsAtStartup` in appsettings.json).

## Lead intake

Submissions to `/start-project` are:

1. Validated server-side (required fields, email/URL formats, select values re-checked against server whitelists).
2. Saved to SQL Server with `SubmittedAtUtc` and status **New Lead**, plus a status-history row.
3. Emailed to `hello@launchassiststudio.com` as a formatted internal notification.
4. Acknowledged to the prospect with a professional confirmation email.

The lead schema already includes `LeadNotes`, `LeadStatusHistory`, `AssignedTo`, `ConvertedAtUtc`, etc., so lead statuses, notes, follow-up history, assignment and conversion tracking can be built on top without schema rework.

### Anti-spam

- Honeypot field (hidden from humans; bots that fill it are silently dropped)
- Minimum fill-time check (submissions faster than 4 seconds are dropped)
- Per-IP rate limiting on the form POST (5 submissions / 10 minutes)
- ASP.NET Core antiforgery tokens

## Email: Resend (default provider)

Transactional mail goes through the **Resend API**. `Email:Provider` selects the implementation — `Resend` (default), `Mailtrap`, or `Smtp` — and all three sit behind the same `IEmailSender` interface, so the intake form is unchanged.

`ResendEmailSender` posts to the Resend endpoint directly with `HttpClient` — **no SDK package**, so the project restores from nuget.org alone.

Only two settings are required:

| Setting | Purpose |
|---|---|
| `Email:Provider` | `Resend` (default), `Mailtrap`, or `Smtp` |
| `Email:Resend:ApiKey` | Resend → API Keys (`re_...`) |

`Email:Resend:Tag` tags every message so this site's mail can be filtered in the dashboard — give each site its own value. `Email:Resend:SendEndpoint` defaults to `https://api.resend.com/emails`.

Sending from `hello@launchassiststudio.com` requires verifying `launchassiststudio.com` in Resend (DKIM + SPF DNS records). Note that Cloudflare Email Routing already publishes an SPF record for inbound forwarding — there may only be **one** SPF record per domain, so the two `include:` values must be merged into a single line.

Sent messages are visible at **https://resend.com/emails**.

If the provider rejects a message the exception is logged and **the lead is still saved** — email failures never lose an inquiry.

### Why not Mailtrap

`MailtrapEmailSender` is still present and working; it is used by another property on this account. Mailtrap's free plan allows a single sending domain, and adding a second costs $15/month — Resend's free tier covers 3,000 emails/month across 3 domains, which suits this site's volume.

## Email configuration (secrets)

Secrets are **not** stored in source control. Until configured, the app logs a warning and skips sending — **leads are still saved to SQL Server either way**.

Production values live in `appsettings.Production.json`, which is **git-ignored** (`appsettings.Production.json.example` is the committed template):

```json
{
  "Email": {
    "Provider": "Resend",
    "Resend": { "ApiKey": "YOUR_RESEND_API_KEY" }
  }
}
```

Every setting can also come from environment variables using `__` as the separator (`Email__Resend__ApiKey`, `Email__Provider`), which is the better option on a shared host.

### SMTP alternative

Set `Email:Provider` to `Smtp` and configure via user secrets in development:

```bash
cd src/LaunchAssistStudio.Web
dotnet user-secrets set "Email:Host" "smtp.example.com"
dotnet user-secrets set "Email:Port" "587"
dotnet user-secrets set "Email:Username" "hello@launchassiststudio.com"
dotnet user-secrets set "Email:Password" "YOUR-SMTP-PASSWORD"
```

In production (IIS/Azure), set the same keys as environment variables (`Email__Host`, `Email__Port`, `Email__Username`, `Email__Password`) or in the host's configuration store.

## Deploying with Visual Studio Web Deploy

1. Open `LaunchAssistStudio.slnx` in Visual Studio 2026.
2. Right-click **LaunchAssistStudio.Web** → **Publish…** → **Web Server (IIS)** → **Web Deploy** and enter your host's Web Deploy credentials (or import the `.publishsettings` file from your hosting provider).
3. In the publish profile settings, set the production connection string for `DefaultConnection` (Publish → Settings → Databases) and enable *"Apply this migration on publish"* if desired — or leave `Database:ApplyMigrationsAtStartup` enabled and the app will migrate itself at startup.
4. Publish. Set the `Email:*` settings on the server as environment variables.

The publish profile you create is saved under `Properties/PublishProfiles/` and is git-ignored if it contains credentials (`*.pubxml.user`).

## Design reference — APPROVED DESIGN v2

The visual source of truth is `LaunchAssistStudio-Approved-Design-v2` (sibling folder):
`assets/approved-brand-reference.png`, `index.html`, `css/site.css`, `assets/launch-assist-logo.svg`.

`wwwroot/css/site.css` is that v2 stylesheet verbatim, plus an appended "Production extensions" block that builds page heroes, pricing cards, the intake form and validation styling **from the same v2 tokens** (`--navy #0D1B2A`, `--blue #2563EB`, `--slate #475569`, `--light #CBD5E1`, `--white #F8FAFC`; Poppins headings, Inter body). Do not restyle — extend the existing design system.

## Static preview (`docs/` → GitHub Pages)

`docs/` holds a **full multi-page static export of the real site** — every page, not just the homepage — published at https://randycdouglas.github.io/LaunchAssistStudio/ (Pages source: branch `main`, folder `/docs`).

It is generated from the running .NET app so the preview can never drift from production. To regenerate after design changes:

```bash
dotnet run --project src/LaunchAssistStudio.Web
```

…then, with the app running, execute `scripts/export-docs.ps1`. The exporter fetches each route, rewrites root-relative URLs to relative `.html` paths, adds a "static design preview" banner, and swaps the intake form's POST for a preview-only confirmation (the static host cannot save leads or send email).

## Favicon & social assets

Generated from the Launch Assist logo mark and served from `wwwroot` (and mirrored into `docs/`): `favicon.svg`, `favicon-32x32.png`, `apple-touch-icon.png` (180×180), `icon-512.png`, `site.webmanifest`, and `og-image.png` (1200×630) used by the Open Graph and Twitter card tags. `scripts/make-icons.ps1` and `scripts/make-og.ps1` regenerate them.
