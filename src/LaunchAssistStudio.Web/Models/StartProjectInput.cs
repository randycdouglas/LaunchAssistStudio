using System.ComponentModel.DataAnnotations;

namespace LaunchAssistStudio.Web.Models;

/// <summary>
/// Server-validated intake form model. Select-list values are re-validated
/// against the whitelists in <see cref="IntakeOptions"/> so posted data can
/// never inject arbitrary values into stored leads or notification emails.
/// </summary>
public class StartProjectInput
{
    // 01 — Services
    public List<string> Services { get; set; } = [];

    // 02 — Business
    [MaxLength(200)]
    [Display(Name = "Business Name")]
    public string? BusinessName { get; set; }

    [MaxLength(400)]
    [Url(ErrorMessage = "Please enter a full website address, e.g. https://example.com.")]
    [Display(Name = "Current Website")]
    public string? CurrentWebsite { get; set; }

    [MaxLength(200)]
    public string? Industry { get; set; }

    [MaxLength(4000)]
    [Display(Name = "What does your business do?")]
    public string? BusinessDescription { get; set; }

    // 03 — Project
    [Required(ErrorMessage = "Please tell us what you're trying to build or improve.")]
    [MinLength(10, ErrorMessage = "Please give us a little more detail (at least 10 characters).")]
    [MaxLength(8000)]
    [Display(Name = "What are you trying to build or improve?")]
    public string ProjectDescription { get; set; } = "";

    // 04 — E-commerce details (shown when an e-commerce service is selected)
    public string? EcommerceSellType { get; set; }
    public string? EcommerceProductCount { get; set; }
    [MaxLength(400)] public string? EcommerceExistingPlatform { get; set; }
    public string? EcommerceInventoryNeeds { get; set; }
    public string? EcommerceShipping { get; set; }
    public string? EcommerceSubscriptions { get; set; }
    [MaxLength(600)] public string? EcommerceIntegrations { get; set; }
    public string? EcommerceMigration { get; set; }

    // 05 — Software details (shown when a software/.NET/SQL/API service is selected)
    public string? SoftwareApplicationType { get; set; }
    public string? SoftwareNewOrExisting { get; set; }
    [MaxLength(400)] public string? SoftwareCurrentTechnology { get; set; }
    public string? SoftwareLoginRequirements { get; set; }
    [MaxLength(600)] public string? SoftwareIntegrations { get; set; }
    public string? SoftwareDataMigration { get; set; }
    [MaxLength(8000)] public string? SoftwareBusinessProblem { get; set; }

    // 06 — Budget & timing
    public string? Budget { get; set; }
    public string? Timeline { get; set; }

    // 07 — Contact
    [Required(ErrorMessage = "Please enter your name.")]
    [MaxLength(200)]
    [Display(Name = "Your Name")]
    public string ContactName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(320)]
    public string Email { get; set; } = "";

    [MaxLength(50)]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string? Phone { get; set; }

    public string? PreferredContact { get; set; }

    [MaxLength(4000)]
    [Display(Name = "Anything else we should know?")]
    public string? AdditionalNotes { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm you understand before submitting.")]
    public bool Agreement { get; set; }
}

/// <summary>Whitelisted option lists shared by the form markup and server-side validation.</summary>
public static class IntakeOptions
{
    public static readonly string[] Services =
    [
        "Website Design & Development",
        "E-Commerce / Online Store",
        "Custom Software Development",
        "Existing Software Improvements",
        ".NET Development",
        "SQL Server / Database Work",
        "API or System Integration",
        "Logo Design",
        "Branding",
        "Ongoing Development Support",
        "I'm Not Sure Yet",
    ];

    public static readonly string[] SellTypes =
        ["Physical products", "Digital products", "Services", "Subscriptions / memberships", "A combination", "Not sure yet"];

    public static readonly string[] ProductCounts =
        ["1–10", "11–50", "51–250", "251–1,000", "1,000+", "Not sure"];

    public static readonly string[] YesNoNotSure = ["Yes", "No", "Not sure yet"];

    public static readonly string[] ShippingOptions =
        ["Yes — we ship products", "No — digital or in-person only", "Local delivery / pickup", "Not sure yet"];

    public static readonly string[] MigrationOptions =
        ["Yes — migrate from an existing store", "No — starting fresh", "Not sure yet"];

    public static readonly string[] SoftwareTypes =
    [
        "Internal business application", "Customer portal", "Employee portal", "SaaS application",
        "Dashboard", "Scheduling system", "Workflow automation", "Reporting system",
        "API integration", "Existing application modernization", "Other",
    ];

    public static readonly string[] NewOrExisting =
        ["New application", "Existing application", "Replacing an existing application", "Not sure yet"];

    public static readonly string[] LoginRequirements =
        ["Yes — users will sign in", "No — no accounts needed", "Not sure yet"];

    public static readonly string[] DataMigrationOptions =
        ["Yes — existing data to migrate", "No — starting fresh", "Not sure yet"];

    public static readonly string[] Budgets =
    [
        "Under $500", "$500 – $1,000", "$1,000 – $2,500", "$2,500 – $5,000",
        "$5,000 – $10,000", "$10,000 – $25,000", "$25,000+", "Not sure yet",
    ];

    public static readonly string[] Timelines =
        ["As soon as possible", "Within 30 days", "Within 1–3 months", "Within 3–6 months", "Researching right now"];

    public static readonly string[] PreferredContacts = ["Email", "Phone", "Either"];

    /// <summary>True when the selected services should reveal the e-commerce questions.</summary>
    public static bool TriggersEcommerce(IEnumerable<string> selected) =>
        selected.Any(s => s.Contains("e-commerce", StringComparison.OrdinalIgnoreCase));

    /// <summary>True when the selected services should reveal the software questions.</summary>
    public static bool TriggersSoftware(IEnumerable<string> selected) =>
        selected.Any(s =>
            s.Contains("software", StringComparison.OrdinalIgnoreCase) ||
            s.Contains(".net", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("sql server", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("api", StringComparison.OrdinalIgnoreCase));
}
