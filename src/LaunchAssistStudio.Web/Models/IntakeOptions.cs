namespace LaunchAssistStudio.Web.Models;

/// <summary>Allow-listed option lists, shared by the form markup and server-side validation.</summary>
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

    public static readonly string[] TaxOptions =
        ["Yes — we need sales tax handled", "No — tax isn't a factor", "Not sure yet"];

    public static readonly string[] PaymentProviders =
        ["Stripe", "PayPal", "Square", "Authorize.Net", "A provider you recommend", "Not sure yet"];

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
