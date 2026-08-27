using System.ComponentModel.DataAnnotations;

namespace LaunchAssistStudio.Web.Data.Entities;

/// <summary>
/// A project inquiry submitted through the public intake form. The schema is
/// intentionally wider than the form so it can grow into pipeline management
/// (statuses, notes, assignment, conversion) without a rebuild.
/// </summary>
public class Lead
{
    public int Id { get; set; }

    /// <summary>Non-sequential identifier safe to expose in URLs or emails.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public DateTime SubmittedAtUtc { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = LeadStatuses.NewLead;

    // Services requested (semicolon-delimited list of the whitelisted options).
    [MaxLength(600)]
    public string ServicesRequested { get; set; } = "";

    // Business
    [MaxLength(200)] public string? BusinessName { get; set; }
    [MaxLength(400)] public string? CurrentWebsite { get; set; }
    [MaxLength(200)] public string? Industry { get; set; }
    [MaxLength(4000)] public string? BusinessDescription { get; set; }

    // Project
    [MaxLength(8000)] public string ProjectDescription { get; set; } = "";

    // E-commerce details
    [MaxLength(100)] public string? EcommerceSellType { get; set; }
    [MaxLength(100)] public string? EcommerceProductCount { get; set; }
    [MaxLength(400)] public string? EcommerceExistingPlatform { get; set; }
    [MaxLength(100)] public string? EcommerceInventoryNeeds { get; set; }
    [MaxLength(100)] public string? EcommerceShipping { get; set; }
    [MaxLength(100)] public string? EcommerceSubscriptions { get; set; }
    [MaxLength(600)] public string? EcommerceIntegrations { get; set; }
    [MaxLength(100)] public string? EcommerceMigration { get; set; }

    // Software details
    [MaxLength(100)] public string? SoftwareApplicationType { get; set; }
    [MaxLength(100)] public string? SoftwareNewOrExisting { get; set; }
    [MaxLength(400)] public string? SoftwareCurrentTechnology { get; set; }
    [MaxLength(100)] public string? SoftwareLoginRequirements { get; set; }
    [MaxLength(600)] public string? SoftwareIntegrations { get; set; }
    [MaxLength(100)] public string? SoftwareDataMigration { get; set; }
    [MaxLength(8000)] public string? SoftwareBusinessProblem { get; set; }

    // Budget & timing
    [MaxLength(100)] public string? Budget { get; set; }
    [MaxLength(100)] public string? Timeline { get; set; }

    // Contact
    [MaxLength(200)] public string ContactName { get; set; } = "";
    [MaxLength(320)] public string Email { get; set; } = "";
    [MaxLength(50)] public string? Phone { get; set; }
    [MaxLength(50)] public string? PreferredContact { get; set; }
    [MaxLength(4000)] public string? AdditionalNotes { get; set; }

    // Pipeline / CRM growth columns
    [MaxLength(200)] public string? AssignedTo { get; set; }
    public DateTime? LastContactedAtUtc { get; set; }
    public DateTime? ConvertedAtUtc { get; set; }
    [MaxLength(100)] public string? Source { get; set; }

    public List<LeadNote> Notes { get; set; } = [];
    public List<LeadStatusHistory> StatusHistory { get; set; } = [];
}
