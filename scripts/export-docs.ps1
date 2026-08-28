# Regenerates docs/ — the GitHub Pages customer preview — from wwwroot.
#
# wwwroot is the real site and uses root-relative URLs (/css/site.css, /services).
# GitHub Pages serves this repo under /LaunchAssistStudio/, so every root-relative
# URL is re-prefixed. The contact form is also neutralised: Pages is static and
# has no /api/contact to post to.

$Repo    = Split-Path $PSScriptRoot -Parent
$WwwRoot = Join-Path $Repo "src\LaunchAssistStudio.Web\wwwroot"
$Docs    = Join-Path $Repo "docs"
$Prefix  = "/LaunchAssistStudio"

if (Test-Path $Docs) { Remove-Item -Recurse -Force $Docs }
Copy-Item -Recurse $WwwRoot $Docs

# web.config is an IIS concern, not a Pages one.
Remove-Item (Join-Path $Docs "web.config") -ErrorAction SilentlyContinue

$formShim = @'
<script>
(function () {
  var form = document.getElementById("contactForm");
  if (!form) return;
  form.addEventListener("submit", function (e) {
    e.preventDefault();
    var card = document.createElement("div");
    card.className = "success-card";
    card.innerHTML =
      '<span class="success-icon">&#10003;</span>' +
      '<h2>Thanks &mdash; that\'s the whole flow.</h2>' +
      '<p>This design preview doesn\'t send anything.</p>' +
      '<p class="muted">On the live site this is validated on the server, then emailed ' +
      'straight to the studio with your address in Reply-To.</p>' +
      '<p style="margin-top:26px"><a class="button primary" href="PREFIX/">Back to Home</a></p>';
    form.replaceWith(card);
    window.scrollTo({ top: 0, behavior: "smooth" });
  });
})();
</script>
'@ -replace 'PREFIX', $Prefix

$count = 0
Get-ChildItem $Docs -Recurse -Filter *.html | ForEach-Object {
    $html = Get-Content $_.FullName -Raw

    # Re-prefix root-relative links/assets for the Pages subpath.
    $html = [regex]::Replace($html, '(?<attr>\b(?:href|src))="/(?!/)(?<rest>[^"]*)"', {
        param($m)
        '{0}="{1}/{2}"' -f $m.Groups['attr'].Value, $Prefix, $m.Groups['rest'].Value
    })

    if ($_.FullName -like "*start-project\index.html") {
        $html = $html -replace '(?i)</body>', "$formShim`n</body>"
    }

    Set-Content -Encoding utf8NoBOM $_.FullName $html
    $count++
}

New-Item -ItemType File -Force (Join-Path $Docs ".nojekyll") | Out-Null

@"
# docs/ — GENERATED, do not hand-edit

The GitHub Pages preview, produced by ``scripts/export-docs.ps1`` from
``src/LaunchAssistStudio.Web/wwwroot``. The whole folder is deleted and rebuilt on
each run, so any manual edit here is lost.

To change the site, edit the files in ``wwwroot`` and re-run the script.

Live preview: https://randycdouglas.github.io/LaunchAssistStudio/
"@ | Set-Content -Encoding utf8NoBOM (Join-Path $Docs "README.md")

"rewrote $count HTML files -> docs/ (prefix $Prefix)"
