namespace LaunchAssistStudio.Web.Models;

/// <summary>
/// A validated inquiry, used only to compose the notification emails. Nothing is
/// persisted — the mailbox is the system of record for this site.
/// </summary>
public class Lead
{
    /// <summary>"project" (full intake) or "general" (short contact form).</summary>
    public bool IsGeneral { get; init; }

    public DateTime SubmittedAtUtc { get; init; }
    public string ServicesRequested { get; init; } = "";

    public string? BusinessName { get; init; }
    public string? CurrentWebsite { get; init; }
    public string? Industry { get; init; }
    public string? BusinessDescription { get; init; }

    public string ProjectDescription { get; init; } = "";

    public string? EcommerceSellType { get; init; }
    public string? EcommerceProductCount { get; init; }
    public string? EcommerceExistingPlatform { get; init; }
    public string? EcommerceInventoryNeeds { get; init; }
    public string? EcommerceShipping { get; init; }
    public string? EcommerceTaxes { get; init; }
    public string? EcommerceSubscriptions { get; init; }
    public string? EcommercePaymentProvider { get; init; }
    public string? EcommerceIntegrations { get; init; }
    public string? EcommerceMigration { get; init; }

    public string? SoftwareApplicationType { get; init; }
    public string? SoftwareNewOrExisting { get; init; }
    public string? SoftwareCurrentTechnology { get; init; }
    public string? SoftwareLoginRequirements { get; init; }
    public string? SoftwareIntegrations { get; init; }
    public string? SoftwareDataMigration { get; init; }
    public string? SoftwareMigrationNeeds { get; init; }
    public string? SoftwareBusinessProblem { get; init; }

    public string? Budget { get; init; }
    public string? Timeline { get; init; }
    public string? TargetLaunchDate { get; init; }

    public string ContactName { get; init; } = "";
    public string Email { get; init; } = "";
    public string? Phone { get; init; }
    public string? PreferredContact { get; init; }
    public string? AdditionalNotes { get; init; }
}
