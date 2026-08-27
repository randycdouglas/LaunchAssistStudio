# Launch Assist Studio — APPROVED DESIGN v2 Handoff

## Why this file exists

The previously published prototype at the GitHub Pages URL drifted away from the visual design the owner approved.

This folder corrects that.

### The visual source of truth is:

1. `assets/approved-brand-reference.png`
2. `index.html`
3. `css/site.css`
4. `assets/launch-assist-logo.svg`

The production .NET site should look like THIS design, not the earlier GitHub Pages version.

## Critical interpretation of the approved image

The approved image is a brand board. The production website should NOT literally reproduce the white brand-board sections or business-card mockups as a webpage.

The website visual target is primarily the website shown INSIDE THE LAPTOP/PHONE in the approved board:

- Dark navy hero
- Compact Launch Assist logo in the upper-left
- Navigation at upper-right
- Blue "LET'S TALK" CTA
- Small blue kicker: `BUILD. LAUNCH. GROW.`
- Exact hero headline:
  `Websites & Custom Software That Drive Real Results.`
- Supporting copy:
  `Professional design. Clean code. Microsoft technologies. Solutions built for your business.`
- Blue software/code interface illustration on the right
- Strong navy + electric blue technical aesthetic
- Clean light sections beneath the hero
- Poppins headings + Inter body text
- Exact base palette:
  - #0D1B2A
  - #2563EB
  - #475569
  - #CBD5E1
  - #F8FAFC

## Build instructions

Convert this reference into .NET 10 using ASP.NET Core.

Web Deploy from Visual Studio must remain straightforward.

Do NOT redesign it.

You may componentize/refactor markup and CSS, but compare the running .NET site side-by-side with `index.html` and `approved-brand-reference.png` before considering the phase complete.

## Preserve these business priorities

Primary:
1. Custom software development
2. Website development
3. E-commerce
4. .NET / SQL Server
5. Branding

Core technologies:
C#, .NET, ASP.NET Core, Blazor, Entity Framework, SQL Server, REST APIs.

## Existing business requirements that still apply

- Dynamic Start a Project form
- SQL Server lead storage
- Internal email notification to hello@launchassiststudio.com
- Customer confirmation email
- Server-side validation
- Anti-spam
- Portfolio with Office Assist AI as the flagship software case study
- SEO-friendly service pages
- Introductory pricing

## Pricing

Keep:
- Logo $99 starting
- Branding $249 starting
- Starter Website $399 starting
- Business Website $699 starting
- Business Launch $899 starting
- E-Commerce $999 starting
- Custom Software $1,500 starting
- Existing Application help $250 starting
- Website Support $49/mo starting
- Development Support $149/mo starting

Pricing does NOT have to dominate the homepage. Put full pricing on the Pricing page.

## First task for Claude

1. Open `approved-brand-reference.png`.
2. Run/open `index.html`.
3. Compare them.
4. Open the currently published GitHub Pages site if useful.
5. Rebuild the .NET homepage so it visually matches THIS v2 reference.
6. Only then convert the other pages using the same design system.
