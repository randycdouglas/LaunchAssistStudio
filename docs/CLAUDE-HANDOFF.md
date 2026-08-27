# Launch Assist Studio — Claude .NET Handoff

## Purpose

This folder is the **visual and content source of truth** for the Launch Assist Studio website.

Open `index.html` in a browser and review the complete static prototype before changing architecture or styling.

The goal is **NOT** to redesign this site. The goal is to faithfully rebuild it as a production .NET application while preserving the approved visual direction.

Also review:

`assets/approved-brand-direction.png`

That image is the approved brand-board direction that led to this web design.

---

## Brand / Visual Requirements

Preserve:

- Dark navy / near-black visual system
- Electric-blue accents
- Modern technical/software-agency aesthetic
- Clean sans-serif typography
- High contrast
- Subtle grid / dashboard / code-inspired visuals
- Responsive/mobile-first behavior
- Premium software-development feel
- Microsoft-focused positioning

The site should look like a **professional digital development studio**, not a generic freelance web designer or marketing agency.

Primary business hierarchy:

1. Custom software development
2. Website development
3. E-commerce development
4. .NET / SQL Server expertise
5. Logo & branding

Core technologies:

- C#
- .NET
- ASP.NET Core
- Blazor
- Web API
- Entity Framework
- SQL Server

---

## Static Reference Files

- `index.html` — homepage
- `services.html` — services
- `pricing.html` — launch pricing
- `portfolio.html` — portfolio shell + Office Assist AI feature
- `about.html`
- `contact.html`
- `start-project.html` — dynamic prototype intake form
- `css/site.css` — approved styling
- `js/site.js` — prototype interactions
- `assets/approved-brand-direction.png` — original approved visual direction

Treat these as design reference assets.

---

## .NET Implementation Goal

Build the production site using .NET 10.

Preferred options:

- ASP.NET Core Razor Pages, or
- Blazor, if there is a strong reason to use it

Choose the simplest maintainable production architecture.

The owner wants to publish using **Visual Studio Web Deploy**, so keep deployment straightforward.

Requirements:

- .NET 10
- Production-ready ASP.NET Core structure
- SQL Server
- Strong server-side validation
- Responsive design
- SEO-friendly routes
- Accessible markup
- Security-conscious form handling
- Easy Visual Studio publish / Web Deploy workflow

---

## Project Intake

The static `start-project.html` demonstrates the intended UX.

In production:

1. Validate input server-side.
2. Save the lead to SQL Server.
3. Store the submission date/time.
4. Default lead status to `New Lead`.
5. Send a formatted internal notification to:
   `hello@launchassiststudio.com`
6. Send the prospect a professional acknowledgement email.
7. Do not expose SMTP/API secrets in source control.
8. Add anti-spam protection appropriate for a public lead form.

Design the lead schema so it can later support:

- Lead statuses
- Notes
- Estimates/proposals
- Projects
- Clients
- Follow-up history
- Assignment
- Conversion tracking

---

## E-Commerce Intake Logic

When `E-Commerce / Online Store` is selected, display the e-commerce questions.

Capture at least:

- What they sell
- Product count
- Existing platform
- Inventory requirements
- Shipping
- Subscriptions
- Integrations
- Migration requirements

---

## Software Intake Logic

When custom software, .NET, SQL Server, API integration, or existing software work is selected, display software questions.

Capture at least:

- Application type
- New vs existing application
- Current technology
- Login/account requirements
- Integrations
- Existing data / migration
- Business problem and desired workflow

---

## Introductory Pricing

Keep these visible as **starting prices** and label them as introductory launch pricing:

- Logo — $99
- Brand Launch — $249
- Starter Website — $399
- Business Website — $699
- Business Launch (website + branding) — $899
- E-Commerce — $999
- Custom Software — $1,500
- Existing Application / Development Help — $250
- Website Support — $49/month
- Development Support — $149/month

Do not imply that every custom project can be completed for the starting price.

---

## Office Assist AI Portfolio Item

Use Office Assist AI as the flagship software case study.

Safe capability-level description:

- Multi-tenant SaaS
- AI integrations
- Voice / SMS telecommunications
- Scheduling
- Payments
- Business dashboards
- SQL Server
- APIs
- Workflow automation

Do not expose proprietary source code, secrets, infrastructure details, credentials, customer data, or sensitive implementation specifics.

---

## SEO Pages To Add

After the main site is stable, add:

- `/web-design`
- `/ecommerce-development`
- `/custom-software-development`
- `/dotnet-development`
- `/sql-server-development`

They should use the same design system and reusable components.

---

## Important

Do not replace the existing visual design with a new theme or template.

The HTML/CSS reference is intentional.

You may refactor the CSS/components for maintainability, but the rendered result should stay visually faithful to the reference.

First milestone:

1. Create/open the .NET 10 solution.
2. Reproduce the reference homepage.
3. Confirm it compiles and runs.
4. Compare it visually against `index.html`.
5. Then move through the remaining pages.

After each major phase, report:

- Files created/changed
- What was implemented
- Any configuration still needed
- Any visual differences from the reference
- Recommended next step
