# Exports the running .NET site to a static site under docs/ for GitHub Pages
# preview. Pages are fetched from the live app so the preview always matches
# production markup; only URLs are rewritten to relative .html paths.

$Base    = "http://localhost:5065"
$Repo    = "C:\Users\rdouglas\source\repos\LaunchAssistStudio"
$Docs    = Join-Path $Repo "docs"
$WwwRoot = Join-Path $Repo "src\LaunchAssistStudio.Web\wwwroot"

# route -> output file
$Pages = [ordered]@{
    "/"                             = "index.html"
    "/services"                     = "services.html"
    "/pricing"                      = "pricing.html"
    "/portfolio"                    = "portfolio.html"
    "/about"                        = "about.html"
    "/contact"                      = "contact.html"
    "/start-project"                = "start-project.html"
    "/start-project/thank-you"      = "thank-you.html"
    "/web-design"                   = "web-design.html"
    "/ecommerce-development"        = "ecommerce-development.html"
    "/custom-software-development"  = "custom-software-development.html"
    "/dotnet-development"           = "dotnet-development.html"
    "/sql-server-development"       = "sql-server-development.html"
    "/logo-branding"                = "logo-branding.html"
}

if (Test-Path $Docs) { Remove-Item -Recurse -Force $Docs }
New-Item -ItemType Directory -Force $Docs | Out-Null

# Static assets
Copy-Item -Recurse (Join-Path $WwwRoot "css")    (Join-Path $Docs "css")
Copy-Item -Recurse (Join-Path $WwwRoot "js")     (Join-Path $Docs "js")
Copy-Item -Recurse (Join-Path $WwwRoot "assets") (Join-Path $Docs "assets")
foreach ($f in @("favicon.svg","favicon-32x32.png","apple-touch-icon.png","icon-512.png","og-image.png","robots.txt")) {
    Copy-Item (Join-Path $WwwRoot $f) (Join-Path $Docs $f)
}

# Preview manifest uses relative icon paths (the preview lives on a subpath)
@'
{
  "name": "Launch Assist Studio",
  "short_name": "Launch Assist",
  "description": "Websites, e-commerce and custom Microsoft .NET software for businesses ready to launch and grow.",
  "start_url": "./",
  "display": "standalone",
  "background_color": "#0D1B2A",
  "theme_color": "#0D1B2A",
  "icons": [
    { "src": "favicon.svg", "type": "image/svg+xml", "sizes": "any", "purpose": "any" },
    { "src": "favicon-32x32.png", "type": "image/png", "sizes": "32x32" },
    { "src": "apple-touch-icon.png", "type": "image/png", "sizes": "180x180" },
    { "src": "icon-512.png", "type": "image/png", "sizes": "512x512" }
  ]
}
'@ | Set-Content -Encoding utf8NoBOM (Join-Path $Docs "site.webmanifest")

# The static preview cannot post the intake form; show the success state instead.
$formShim = @'
<script>
(function(){
  var form = document.querySelector("form.project-form");
  if (!form) return;
  form.addEventListener("submit", function(e){
    e.preventDefault();
    var card = document.createElement("div");
    card.className = "success-card";
    card.innerHTML = '<span class="success-icon">&#10003;</span>' +
      '<h2>Thanks &mdash; that\'s the whole flow.</h2>' +
      '<p>This design preview doesn\'t send or store anything.</p>' +
      '<p class="muted">On the live site, this submission is validated on the server, saved to SQL Server as a <strong>New Lead</strong>, and triggers both an internal notification and your confirmation email.</p>' +
      '<p style="margin-top:26px"><a class="button primary" href="index.html">Back to Home</a></p>';
    form.replaceWith(card);
    window.scrollTo({top: 0, behavior: "smooth"});
  });
})();
</script>
'@

function Convert-Url([string]$raw) {
    $u = $raw.Split('?')[0].TrimStart('/')          # drop asp-append-version query
    if ($u -eq "") { return "index.html" }
    if ($u -like "css/*" -or $u -like "js/*" -or $u -like "assets/*") { return $u }
    if ($u -in @("favicon.svg","favicon-32x32.png","apple-touch-icon.png","icon-512.png","og-image.png","site.webmanifest","robots.txt","sitemap.xml")) { return $u }
    $key = "/$u"
    if ($Pages.Contains($key)) { return $Pages[$key] }
    return $raw   # leave anything unrecognised untouched
}

foreach ($route in $Pages.Keys) {
    $file = $Pages[$route]
    $html = (Invoke-WebRequest -Uri "$Base$route" -UseBasicParsing).Content

    # Rewrite root-relative href/src to relative static paths
    $html = [regex]::Replace($html, '(?<attr>\b(?:href|src))="(?<url>/[^"#]*)"', {
        param($m)
        $new = Convert-Url $m.Groups['url'].Value
        '{0}="{1}"' -f $m.Groups['attr'].Value, $new
    })

    if ($file -eq "start-project.html") {
        $html = $html -replace '(?i)</body>', "$formShim`n</body>"
    }

    Set-Content -Encoding utf8NoBOM (Join-Path $Docs $file) $html
    "exported {0,-38} -> {1}" -f $route, $file
}

# Prevent Jekyll from touching the static output
New-Item -ItemType File -Force (Join-Path $Docs ".nojekyll") | Out-Null
"wrote .nojekyll"

@'
# docs/ — GENERATED, do not hand-edit

These files are the static GitHub Pages preview of the site. They are produced
by `scripts/export-docs.ps1`, which fetches every page from the running .NET app
and rewrites URLs to relative .html paths. The whole folder is deleted and
rebuilt on each run, so any manual edit here is lost.

To change the site:

1. Edit the Razor Pages under `src/LaunchAssistStudio.Web/Pages`.
2. Run the app: `dotnet run --project src/LaunchAssistStudio.Web`
3. With it running, execute `scripts/export-docs.ps1`.
4. Commit and push; GitHub Pages serves this folder from branch `main`.

Live preview: https://randycdouglas.github.io/LaunchAssistStudio/
'@ | Set-Content -Encoding utf8NoBOM (Join-Path $Docs "README.md")
"wrote README.md (generated-folder notice)"
