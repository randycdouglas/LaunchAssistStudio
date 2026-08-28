# Launch Assist Studio — Website

Production website for **Launch Assist Studio** (launchassiststudio.com): websites, custom .NET software, e-commerce and branding.

The site is **static HTML** served by a minimal **.NET 10 / ASP.NET Core** app. The only server-side code is the contact form: two endpoints, no database, no ORM.

## Solution layout

```
LaunchAssistStudio.slnx
src/LaunchAssistStudio.Web/
  Program.cs        Static file hosting + /api/health + /api/contact
  Models/           ContactRequest (validation), Lead, IntakeOptions (allow-lists)
  Services/         IEmailSender -> ResendEmailSender | SmtpEmailSender,
                    LeadEmailComposer, TurnstileVerifier
  wwwroot/          THE SITE — one folder per page, plus css/, js/, assets/
  web.config        IIS: ASP.NET Core module + MIME types
scripts/            Icon and OG image generators
```

Everything the visitor sees lives in `wwwroot`. Pages are folders so URLs stay extensionless — `wwwroot/services/index.html` is served at `/services`.

Published output is 4 DLLs / ~8 MB; three of those are the optional SMTP fallback (MailKit). Resend needs no package at all.

## Pages

| Route | Page |
|---|---|
| `/` | Home |
| `/services`, `/pricing`, `/portfolio`, `/about`, `/contact` | Main site |
| `/start-project`, `/start-project/thank-you` | Intake form and confirmation |
| `/web-design`, `/ecommerce-development`, `/custom-software-development`, `/dotnet-development`, `/sql-server-development`, `/logo-branding` | SEO landing pages |

## API

### `GET /api/health`

Reports which transport is active and what is still missing — **names only, never values**, so it is safe to open in a browser.

```json
{ "status": "unconfigured", "transport": "Resend",
  "missingSettings": ["Email:Resend:ApiKey"], "turnstile": "disabled" }
```

### `POST /api/contact`

JSON in, JSON out. The page posts to it with `fetch`; the server re-validates everything.

| Outcome | Status | Behaviour |
|---|---|---|
| Valid | 200 | Notification + acknowledgement sent, page redirects to the thank-you route |
| Invalid | 400 | `errors` keyed by field, rendered above the form |
| Honeypot filled | 200 | Discarded silently — bots get no signal to adapt to |
| Send failed | 502 | Visitor is shown the studio's email address; reason is logged |
| Too many posts | 429 | 12 per IP per 10 minutes |

## Email

`Email:Provider` selects the transport behind `IEmailSender`:

| Value | Transport | Needs |
|---|---|---|
| `Resend` (default) | Resend API over HTTPS, no package | `Email:Resend:ApiKey` |
| `Smtp` | MailKit | `Email:Smtp:Host`, `Username`, `Password` |

Mail is always sent **From** the verified sender address, with the visitor in **Reply-To**. Putting the visitor in `From` would fail SPF/DKIM and land the studio's own intake in spam.

Sending from `hello@launchassiststudio.com` requires the domain verified in Resend. Resend publishes its SPF and bounce records on the `send.` subdomain, so they sit alongside the Cloudflare Email Routing records at the apex without conflicting — no SPF merge is needed.

Delivery log: **https://resend.com/emails**

## Configuration and secrets

Nothing secret is committed. Settings live under the `Email` section of `appsettings.json`; real values go in `appsettings.Production.json` on the server (git-ignored — `appsettings.Production.json.example` is the template), or as environment variables in the host control panel:

```
Email__Provider=Resend
Email__Resend__ApiKey=re_...
```

Shipped placeholders (`REPLACE_WITH_...`, `<goes here>`) are treated as **unset**, so a half-finished config reports itself broken on `/api/health` instead of appearing to work.

## Contact form defences

- Server-side validation; dropdowns re-checked against allow-lists in `IntakeOptions`
- Length caps on every field
- Control characters stripped; single-line fields (name, business name — both feed the subject) have newlines collapsed, which is what blocks header injection
- Hidden honeypot that returns 200 while discarding
- Per-IP rate limiting
- Cloudflare Turnstile, dormant until `Email:Turnstile:SecretKey` is set

## Running locally

```bash
dotnet run --project src/LaunchAssistStudio.Web
```

```bash
curl -s localhost:5065/api/health
```

On Windows a running app locks its binaries — stop it before rebuilding, or the build fails with MSB3027:

```bash
powershell -Command "Get-Process -Name 'LaunchAssistStudio.Web' -ErrorAction SilentlyContinue | Stop-Process -Force"
```

## Deploying (Visual Studio Publish / Web Deploy)

1. Right-click the project → **Publish** → Web Deploy or FTP to the IIS host.
2. Set the site's application pool to **No Managed Code** (`web.config` uses `hostingModel="inprocess"`).
3. Confirm the host's per-site .NET version matches `net10.0`, or IIS returns 500.19 / 502.5.
4. Put the Resend API key in `appsettings.Production.json` on the server, or set `Email__Resend__ApiKey` in the control panel.
5. Open `/api/health` and confirm `"status": "ok"`.

