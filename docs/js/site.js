(() => {
    const toggle = document.querySelector(".menu-toggle");
    const nav = document.querySelector(".main-nav");

    if (toggle && nav) {
        toggle.addEventListener("click", () => {
            const open = nav.classList.toggle("open");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
        });
    }

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

    // Static prototype only: prevent the sample form from posting anywhere.
    const form = document.querySelector(".project-form");
    if (form) {
        form.addEventListener("submit", (e) => {
            e.preventDefault();
            const card = document.createElement("div");
            card.className = "success-card";
            card.innerHTML = `
                <span class="success-icon">✓</span>
                <h2>Prototype submission complete.</h2>
                <p>This static HTML reference does not send or store data.</p>
                <p class="muted">Claude should wire the .NET version to SQL Server and email.</p>
                <a class="btn btn-primary" href="index.html">Back to Home</a>`;
            form.replaceWith(card);
        });
    }
})();
