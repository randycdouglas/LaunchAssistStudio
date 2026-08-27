(() => {
    const toggle = document.querySelector(".menu-toggle");
    const nav = document.querySelector(".main-nav");

    if (toggle && nav) {
        toggle.addEventListener("click", () => {
            const open = nav.classList.toggle("open");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
        });
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
