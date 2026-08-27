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
