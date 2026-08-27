using LaunchAssistStudio.Web.Data;
using LaunchAssistStudio.Web.Data.Entities;
using LaunchAssistStudio.Web.Models;
using LaunchAssistStudio.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace LaunchAssistStudio.Web.Pages;

public class StartProjectModel(
    AppDbContext db,
    IEmailSender emailSender,
    IOptions<EmailOptions> emailOptions,
    ILogger<StartProjectModel> logger) : PageModel
{
    // Bots that fill every field are dropped silently; humans never see this field.
    private const string HoneypotFieldName = "CompanyFax";

    // A human takes longer than this to fill in a seven-section form.
    private static readonly TimeSpan MinimumFillTime = TimeSpan.FromSeconds(4);

    [BindProperty]
    public StartProjectInput Input { get; set; } = new();

    [BindProperty(Name = HoneypotFieldName)]
    public string? Honeypot { get; set; }

    [BindProperty]
    public long FormRenderedAt { get; set; }

    public bool ShowEcommerce { get; private set; }
    public bool ShowSoftware { get; private set; }

    public void OnGet()
    {
        FormRenderedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        // Anti-spam: honeypot filled or form submitted implausibly fast.
        // Pretend success so bots get no signal to adapt to.
        var elapsed = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(FormRenderedAt);
        if (!string.IsNullOrWhiteSpace(Honeypot) || elapsed < MinimumFillTime || elapsed > TimeSpan.FromHours(24))
        {
            logger.LogWarning("Dropped suspected spam submission (honeypot={HoneypotFilled}, elapsed={Elapsed}).",
                !string.IsNullOrWhiteSpace(Honeypot), elapsed);
            return RedirectToPage("StartProjectThanks");
        }

        // Re-validate every choice-based field against the server-side whitelists.
        Input.Services = Input.Services
            .Where(s => IntakeOptions.Services.Contains(s))
            .Distinct()
            .ToList();

        Input.EcommerceSellType = Whitelist(Input.EcommerceSellType, IntakeOptions.SellTypes);
        Input.EcommerceProductCount = Whitelist(Input.EcommerceProductCount, IntakeOptions.ProductCounts);
        Input.EcommerceInventoryNeeds = Whitelist(Input.EcommerceInventoryNeeds, IntakeOptions.YesNoNotSure);
        Input.EcommerceShipping = Whitelist(Input.EcommerceShipping, IntakeOptions.ShippingOptions);
        Input.EcommerceSubscriptions = Whitelist(Input.EcommerceSubscriptions, IntakeOptions.YesNoNotSure);
        Input.EcommerceMigration = Whitelist(Input.EcommerceMigration, IntakeOptions.MigrationOptions);
        Input.SoftwareApplicationType = Whitelist(Input.SoftwareApplicationType, IntakeOptions.SoftwareTypes);
        Input.SoftwareNewOrExisting = Whitelist(Input.SoftwareNewOrExisting, IntakeOptions.NewOrExisting);
        Input.SoftwareLoginRequirements = Whitelist(Input.SoftwareLoginRequirements, IntakeOptions.LoginRequirements);
        Input.SoftwareDataMigration = Whitelist(Input.SoftwareDataMigration, IntakeOptions.DataMigrationOptions);
        Input.Budget = Whitelist(Input.Budget, IntakeOptions.Budgets);
        Input.Timeline = Whitelist(Input.Timeline, IntakeOptions.Timelines);
        Input.PreferredContact = Whitelist(Input.PreferredContact, IntakeOptions.PreferredContacts);

        if (Input.Services.Count == 0)
        {
            ModelState.AddModelError("Input.Services", "Please select at least one service so we know how to help.");
        }

        ShowEcommerce = IntakeOptions.TriggersEcommerce(Input.Services);
        ShowSoftware = IntakeOptions.TriggersSoftware(Input.Services);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var lead = new Lead
        {
            SubmittedAtUtc = DateTime.UtcNow,
            Status = LeadStatuses.NewLead,
            Source = "Website intake form",
            ServicesRequested = string.Join("; ", Input.Services),

            BusinessName = Clean(Input.BusinessName),
            CurrentWebsite = Clean(Input.CurrentWebsite),
            Industry = Clean(Input.Industry),
            BusinessDescription = Clean(Input.BusinessDescription),

            ProjectDescription = Input.ProjectDescription.Trim(),

            EcommerceSellType = Input.EcommerceSellType,
            EcommerceProductCount = Input.EcommerceProductCount,
            EcommerceExistingPlatform = Clean(Input.EcommerceExistingPlatform),
            EcommerceInventoryNeeds = Input.EcommerceInventoryNeeds,
            EcommerceShipping = Input.EcommerceShipping,
            EcommerceSubscriptions = Input.EcommerceSubscriptions,
            EcommerceIntegrations = Clean(Input.EcommerceIntegrations),
            EcommerceMigration = Input.EcommerceMigration,

            SoftwareApplicationType = Input.SoftwareApplicationType,
            SoftwareNewOrExisting = Input.SoftwareNewOrExisting,
            SoftwareCurrentTechnology = Clean(Input.SoftwareCurrentTechnology),
            SoftwareLoginRequirements = Input.SoftwareLoginRequirements,
            SoftwareIntegrations = Clean(Input.SoftwareIntegrations),
            SoftwareDataMigration = Input.SoftwareDataMigration,
            SoftwareBusinessProblem = Clean(Input.SoftwareBusinessProblem),

            Budget = Input.Budget,
            Timeline = Input.Timeline,

            ContactName = Input.ContactName.Trim(),
            Email = Input.Email.Trim(),
            Phone = Clean(Input.Phone),
            PreferredContact = Input.PreferredContact,
            AdditionalNotes = Clean(Input.AdditionalNotes),
        };

        lead.StatusHistory.Add(new LeadStatusHistory
        {
            FromStatus = null,
            ToStatus = LeadStatuses.NewLead,
            ChangedAtUtc = lead.SubmittedAtUtc,
            ChangedBy = "System (form submission)",
        });

        db.Leads.Add(lead);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("New lead {PublicId} saved from {Email}.", lead.PublicId, lead.Email);

        // Email failures must never lose a saved lead — log and continue.
        try
        {
            var (subject, body) = LeadEmailComposer.BuildInternalNotification(lead);
            await emailSender.SendAsync(emailOptions.Value.InternalNotificationAddress, "Launch Assist Studio", subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send internal notification for lead {PublicId}.", lead.PublicId);
        }

        try
        {
            var (subject, body) = LeadEmailComposer.BuildProspectAcknowledgement(lead);
            await emailSender.SendAsync(lead.Email, lead.ContactName, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send acknowledgement email for lead {PublicId}.", lead.PublicId);
        }

        return RedirectToPage("StartProjectThanks");
    }

    private static string? Whitelist(string? value, string[] allowed) =>
        !string.IsNullOrWhiteSpace(value) && allowed.Contains(value) ? value : null;

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
