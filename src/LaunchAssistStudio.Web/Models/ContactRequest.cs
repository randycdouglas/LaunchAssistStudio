using System.Text.RegularExpressions;

namespace LaunchAssistStudio.Web.Models;

/// <summary>
/// Payload posted by the intake form. Everything is validated server-side and
/// choice fields are re-checked against the allow-lists in <see cref="IntakeOptions"/>,
/// so a crafted request cannot inject arbitrary values into the notification email.
/// </summary>
public class ContactRequest
{
    public List<string>? Services { get; set; }

    public string? BusinessName { get; set; }
    public string? CurrentWebsite { get; set; }
    public string? Industry { get; set; }
    public string? BusinessDescription { get; set; }

    public string? ProjectDescription { get; set; }

    public string? EcommerceSellType { get; set; }
    public string? EcommerceProductCount { get; set; }
    public string? EcommerceExistingPlatform { get; set; }
    public string? EcommerceInventoryNeeds { get; set; }
    public string? EcommerceShipping { get; set; }
    public string? EcommerceTaxes { get; set; }
    public string? EcommerceSubscriptions { get; set; }
    public string? EcommercePaymentProvider { get; set; }
    public string? EcommerceIntegrations { get; set; }
    public string? EcommerceMigration { get; set; }

    public string? SoftwareApplicationType { get; set; }
    public string? SoftwareNewOrExisting { get; set; }
    public string? SoftwareCurrentTechnology { get; set; }
    public string? SoftwareLoginRequirements { get; set; }
    public string? SoftwareIntegrations { get; set; }
    public string? SoftwareDataMigration { get; set; }
    public string? SoftwareMigrationNeeds { get; set; }
    public string? SoftwareBusinessProblem { get; set; }

    public string? Budget { get; set; }
    public string? Timeline { get; set; }
    public string? TargetLaunchDate { get; set; }

    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PreferredContact { get; set; }
    public string? AdditionalNotes { get; set; }

    public bool Agreement { get; set; }

    /// <summary>Hidden honeypot. Any value means a bot filled the form.</summary>
    public string? CompanyFax { get; set; }

    /// <summary>Cloudflare Turnstile token, when Turnstile is configured.</summary>
    public string? TurnstileToken { get; set; }

    private const int ShortText = 200;
    private const int MediumText = 600;
    private const int LongText = 8000;

    public Dictionary<string, string> Validate()
    {
        var errors = new Dictionary<string, string>();

        var name = Clean(ContactName, ShortText);
        if (string.IsNullOrWhiteSpace(name)) errors["contactName"] = "Please enter your name.";

        var email = Clean(Email, 320);
        if (string.IsNullOrWhiteSpace(email)) errors["email"] = "Please enter your email address.";
        else if (!IsValidEmail(email)) errors["email"] = "Please enter a valid email address.";

        var project = Clean(ProjectDescription, LongText, multiline: true);
        if (string.IsNullOrWhiteSpace(project)) errors["projectDescription"] = "Please tell us what you're trying to build or improve.";
        else if (project.Length < 10) errors["projectDescription"] = "Please give us a little more detail.";

        var services = SelectedServices();
        if (services.Count == 0) errors["services"] = "Please select at least one service.";

        if (!Agreement) errors["agreement"] = "Please confirm you understand before submitting.";

        var website = Clean(CurrentWebsite, MediumText);
        if (!string.IsNullOrWhiteSpace(website) &&
            !Uri.TryCreate(website, UriKind.Absolute, out var uri) ||
            (!string.IsNullOrWhiteSpace(website) && Uri.TryCreate(website, UriKind.Absolute, out var u2) &&
             u2.Scheme != Uri.UriSchemeHttp && u2.Scheme != Uri.UriSchemeHttps))
        {
            if (!string.IsNullOrWhiteSpace(website))
            {
                errors["currentWebsite"] = "Please enter a full website address, e.g. https://example.com.";
            }
        }

        if (!string.IsNullOrWhiteSpace(TargetLaunchDate) && !DateOnly.TryParse(TargetLaunchDate, out _))
        {
            errors["targetLaunchDate"] = "Please enter a valid date.";
        }

        return errors;
    }

    public List<string> SelectedServices() =>
        (Services ?? []).Where(IntakeOptions.Services.Contains).Distinct().ToList();

    public Lead ToLead() => new()
    {
        SubmittedAtUtc = DateTime.UtcNow,
        ServicesRequested = string.Join("; ", SelectedServices()),

        BusinessName = Clean(BusinessName, ShortText),
        CurrentWebsite = Clean(CurrentWebsite, MediumText),
        Industry = Clean(Industry, ShortText),
        BusinessDescription = Clean(BusinessDescription, LongText, multiline: true),

        ProjectDescription = Clean(ProjectDescription, LongText, multiline: true) ?? "",

        EcommerceSellType = Allow(EcommerceSellType, IntakeOptions.SellTypes),
        EcommerceProductCount = Allow(EcommerceProductCount, IntakeOptions.ProductCounts),
        EcommerceExistingPlatform = Clean(EcommerceExistingPlatform, MediumText),
        EcommerceInventoryNeeds = Allow(EcommerceInventoryNeeds, IntakeOptions.YesNoNotSure),
        EcommerceShipping = Allow(EcommerceShipping, IntakeOptions.ShippingOptions),
        EcommerceTaxes = Allow(EcommerceTaxes, IntakeOptions.TaxOptions),
        EcommerceSubscriptions = Allow(EcommerceSubscriptions, IntakeOptions.YesNoNotSure),
        EcommercePaymentProvider = Allow(EcommercePaymentProvider, IntakeOptions.PaymentProviders),
        EcommerceIntegrations = Clean(EcommerceIntegrations, MediumText),
        EcommerceMigration = Allow(EcommerceMigration, IntakeOptions.MigrationOptions),

        SoftwareApplicationType = Allow(SoftwareApplicationType, IntakeOptions.SoftwareTypes),
        SoftwareNewOrExisting = Allow(SoftwareNewOrExisting, IntakeOptions.NewOrExisting),
        SoftwareCurrentTechnology = Clean(SoftwareCurrentTechnology, MediumText),
        SoftwareLoginRequirements = Allow(SoftwareLoginRequirements, IntakeOptions.LoginRequirements),
        SoftwareIntegrations = Clean(SoftwareIntegrations, MediumText),
        SoftwareDataMigration = Allow(SoftwareDataMigration, IntakeOptions.DataMigrationOptions),
        SoftwareMigrationNeeds = Clean(SoftwareMigrationNeeds, MediumText),
        SoftwareBusinessProblem = Clean(SoftwareBusinessProblem, LongText, multiline: true),

        Budget = Allow(Budget, IntakeOptions.Budgets),
        Timeline = Allow(Timeline, IntakeOptions.Timelines),
        TargetLaunchDate = DateOnly.TryParse(TargetLaunchDate, out var d) ? d.ToString("yyyy-MM-dd") : null,

        ContactName = Clean(ContactName, ShortText) ?? "",
        Email = Clean(Email, 320) ?? "",
        Phone = Clean(Phone, 50),
        PreferredContact = Allow(PreferredContact, IntakeOptions.PreferredContacts),
        AdditionalNotes = Clean(AdditionalNotes, LongText, multiline: true),
    };

    /// <summary>
    /// Trims, caps length and strips control characters.
    /// <para>
    /// Single-line by default: newlines are collapsed to spaces. That is what stops
    /// a crafted name or business name — both of which feed the email subject — from
    /// injecting extra headers. Only genuine free-text areas pass
    /// <paramref name="multiline"/>, and even those never keep CR.
    /// </para>
    /// </summary>
    private static string? Clean(string? value, int maxLength, bool multiline = false)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        // Turn line breaks into spaces first, so a pasted multi-line value keeps its
        // word boundaries instead of running together once the breaks are removed.
        var normalized = multiline
            ? value.Replace("\r\n", "\n").Replace('\r', '\n')
            : value.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');

        var kept = normalized.Where(c => !char.IsControl(c) || (multiline && c is '\n') || c == '\t');
        var stripped = new string(kept.ToArray());

        if (!multiline)
        {
            // Collapse the runs of whitespace those replacements can leave behind.
            stripped = string.Join(' ', stripped.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        stripped = stripped.Trim();
        if (stripped.Length > maxLength) stripped = stripped[..maxLength];
        return stripped.Length == 0 ? null : stripped;
    }

    /// <summary>Choice fields must match the allow-list exactly, after single-line cleaning.</summary>
    private static string? Allow(string? value, string[] allowed)
    {
        var cleaned = Clean(value, ShortText);
        return cleaned is not null && allowed.Contains(cleaned) ? cleaned : null;
    }

    private static bool IsValidEmail(string value)
    {
        if (value.Any(char.IsControl) || value.Contains(' ')) return false;
        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$", RegexOptions.None, TimeSpan.FromSeconds(1));
    }
}

public record ContactResponse(bool Success, string Message, Dictionary<string, string>? Errors = null);
