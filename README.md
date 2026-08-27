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

## Email configuration (secrets)

SMTP settings are **not** stored in source control. Until configured, the app logs a warning and skips sending (leads are still saved). Configure via user secrets in development:

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

## Design reference

The visual design is the approved reference in `LaunchAssistStudio-HTML-Design-Reference` (sibling repo/folder). `wwwroot/css/site.css` is the reference stylesheet ported verbatim plus a small appended section for validation/anti-spam styling. Do not restyle; extend the existing design system.
