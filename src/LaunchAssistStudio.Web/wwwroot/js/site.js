(() => {
    const toggle = document.querySelector(".nav-toggle");
    const links = document.querySelector(".links");

    if (toggle && links) {
        toggle.addEventListener("click", () => links.classList.toggle("show"));
    }

    // Progressive enhancement: reveal the e-commerce / software question groups
    // as services are selected. The server re-applies the same logic on postback.
    const serviceCheckboxes = document.querySelectorAll(".service-checkbox");
    const ecommerce = document.getElementById("ecommerceQuestions");
    const software = document.getElementById("softwareQuestions");

    function updateConditionalSections() {
        if (!serviceCheckboxes.length) return;

        const selected = [...serviceCheckboxes]
            .filter(x => x.checked)
            .map(x => x.value.toLowerCase());

        const ecommerceSelected = selected.some(x => x.includes("e-commerce"));
        const softwareSelected = selected.some(x =>
            x.includes("software") ||
            x.includes(".net") ||
            x.includes("sql server") ||
            x.includes("api")
        );

        if (ecommerce) ecommerce.classList.toggle("visible", ecommerceSelected);
        if (software) software.classList.toggle("visible", softwareSelected);
    }

    serviceCheckboxes.forEach(x => x.addEventListener("change", updateConditionalSections));
    updateConditionalSections();
})();

// ---------------------------------------------------------------------------
// Intake form -> POST /api/contact
// The server re-validates everything; this only shapes the payload and renders
// whatever the API reports back.
// ---------------------------------------------------------------------------
(() => {
    const form = document.getElementById("contactForm");
    if (!form) return;

    const summary = form.querySelector(".validation-summary");
    const submitButton = form.querySelector('button[type="submit"]');

    const escapeHtml = (s) => String(s).replace(/[&<>"']/g, c =>
        ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

    const setSummary = (message, errors) => {
        if (!summary) return;
        const items = errors && Object.keys(errors).length
            ? Object.values(errors)
            : (message ? [message] : []);
        summary.innerHTML = items.length
            ? "<ul>" + items.map(t => `<li>${escapeHtml(t)}</li>`).join("") + "</ul>"
            : "";
        summary.classList.toggle("validation-summary-valid", items.length === 0);
        if (items.length) summary.scrollIntoView({ behavior: "smooth", block: "center" });
    };

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        setSummary(null, null);

        const data = new FormData(form);
        const payload = {};
        for (const [key, value] of data.entries()) {
            if (key === "Services") continue;
            payload[key] = value;
        }
        payload.Kind = form.dataset.kind || "project";
        payload.Services = data.getAll("Services");
        payload.Agreement = form.querySelector('[name="Agreement"]')?.checked === true;

        const token = form.querySelector('[name="cf-turnstile-response"]');
        if (token) payload.TurnstileToken = token.value;

        const original = submitButton ? submitButton.textContent : null;
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.textContent = "Sending...";
        }

        try {
            const response = await fetch("/api/contact", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload),
            });

            let result = null;
            try { result = await response.json(); } catch { /* non-JSON error page */ }

            if (response.ok && result && result.success) {
                if (payload.Kind === "general") {
                    // Short enquiry: confirm in place rather than sending the visitor
                    // to a page that talks about project inquiries.
                    const card = document.createElement("div");
                    card.className = "success-card";
                    card.innerHTML =
                        '<span class="success-icon">&#10003;</span>' +
                        "<h2>Message sent.</h2>" +
                        "<p>Thanks &mdash; we\x27ll get back to you within one business day.</p>";
                    form.replaceWith(card);
                    window.scrollTo({ top: 0, behavior: "smooth" });
                } else {
                    window.location.href = "/start-project/thank-you";
                }
                return;
            }

            setSummary(
                (result && result.message) || "Sorry - something went wrong. Please email hello@launchassiststudio.com.",
                result && result.errors);
        } catch {
            setSummary("We couldn't reach the server. Please check your connection, or email hello@launchassiststudio.com.", null);
        } finally {
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.textContent = original;
            }
        }
    });
})();
