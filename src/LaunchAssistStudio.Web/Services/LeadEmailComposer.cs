using System.Text;
using LaunchAssistStudio.Web.Models;

namespace LaunchAssistStudio.Web.Services;

/// <summary>Builds the internal notification and prospect acknowledgement emails for a new lead.</summary>
public static class LeadEmailComposer
{
    public static (string Subject, string Body) BuildInternalNotification(Lead lead)
    {
        var subject = $"New Lead: {lead.ContactName}" +
                      (string.IsNullOrWhiteSpace(lead.BusinessName) ? "" : $" — {lead.BusinessName}");

        var sb = new StringBuilder();
        sb.AppendLine("NEW PROJECT INQUIRY — LAUNCH ASSIST STUDIO");
        sb.AppendLine("==========================================");
        sb.AppendLine();
        sb.AppendLine($"Submitted (UTC): {lead.SubmittedAtUtc:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Reply to:        {lead.Email}");
        sb.AppendLine();

        Section(sb, "SERVICES REQUESTED");
        sb.AppendLine(string.IsNullOrWhiteSpace(lead.ServicesRequested) ? "(none selected)" : lead.ServicesRequested);

        Section(sb, "CONTACT");
        Field(sb, "Name", lead.ContactName);
        Field(sb, "Email", lead.Email);
        Field(sb, "Phone", lead.Phone);
        Field(sb, "Preferred contact", lead.PreferredContact);

        Section(sb, "BUSINESS");
        Field(sb, "Business name", lead.BusinessName);
        Field(sb, "Current website", lead.CurrentWebsite);
        Field(sb, "Industry", lead.Industry);
        Field(sb, "About the business", lead.BusinessDescription);

        Section(sb, "PROJECT");
        sb.AppendLine(lead.ProjectDescription);

        if (HasAny(lead.EcommerceSellType, lead.EcommerceProductCount, lead.EcommerceExistingPlatform,
                lead.EcommerceInventoryNeeds, lead.EcommerceShipping, lead.EcommerceSubscriptions,
                lead.EcommerceIntegrations, lead.EcommerceMigration, lead.EcommerceTaxes,
                lead.EcommercePaymentProvider))
        {
            Section(sb, "E-COMMERCE DETAILS");
            Field(sb, "What they sell", lead.EcommerceSellType);
            Field(sb, "Product count", lead.EcommerceProductCount);
            Field(sb, "Existing platform", lead.EcommerceExistingPlatform);
            Field(sb, "Inventory needs", lead.EcommerceInventoryNeeds);
            Field(sb, "Shipping", lead.EcommerceShipping);
            Field(sb, "Sales tax", lead.EcommerceTaxes);
            Field(sb, "Subscriptions", lead.EcommerceSubscriptions);
            Field(sb, "Payment provider", lead.EcommercePaymentProvider);
            Field(sb, "Integrations", lead.EcommerceIntegrations);
            Field(sb, "Migration", lead.EcommerceMigration);
        }

        if (HasAny(lead.SoftwareApplicationType, lead.SoftwareNewOrExisting, lead.SoftwareCurrentTechnology,
                lead.SoftwareLoginRequirements, lead.SoftwareIntegrations, lead.SoftwareDataMigration,
                lead.SoftwareMigrationNeeds, lead.SoftwareBusinessProblem))
        {
            Section(sb, "SOFTWARE DETAILS");
            Field(sb, "Application type", lead.SoftwareApplicationType);
            Field(sb, "New or existing", lead.SoftwareNewOrExisting);
            Field(sb, "Current technology", lead.SoftwareCurrentTechnology);
            Field(sb, "Login/accounts", lead.SoftwareLoginRequirements);
            Field(sb, "Integrations", lead.SoftwareIntegrations);
            Field(sb, "Existing data", lead.SoftwareDataMigration);
            Field(sb, "Migration needs", lead.SoftwareMigrationNeeds);
            Field(sb, "Business problem & workflow", lead.SoftwareBusinessProblem);
        }

        Section(sb, "BUDGET & TIMING");
        Field(sb, "Budget", lead.Budget);
        Field(sb, "Timeline", lead.Timeline);
        Field(sb, "Target launch date", lead.TargetLaunchDate);

        if (!string.IsNullOrWhiteSpace(lead.AdditionalNotes))
        {
            Section(sb, "ADDITIONAL NOTES");
            sb.AppendLine(lead.AdditionalNotes);
        }

        return (subject, sb.ToString());
    }

    public static (string Subject, string Body) BuildProspectAcknowledgement(Lead lead)
    {
        const string subject = "We received your project inquiry — Launch Assist Studio";

        var firstName = lead.ContactName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? lead.ContactName;

        var body = $"""
            Hi {firstName},

            Thank you for reaching out to Launch Assist Studio — we received your project inquiry.

            Here's what happens next:

            1. We review your answers so our first conversation starts from a real understanding of your project.
            2. We follow up within one business day{(string.IsNullOrWhiteSpace(lead.PreferredContact) ? "" : $" by {lead.PreferredContact.ToLowerInvariant()}")} to talk through goals, scope and options.
            3. If it's a fit, we outline a clear plan and pricing before any commitment.

            If anything comes to mind in the meantime, just reply to this email.


            Talk soon,

            Launch Assist Studio
            Websites • Custom Software • E-Commerce • Branding
            hello@launchassiststudio.com
            launchassiststudio.com
            """;

        return (subject, body);
    }

    private static void Section(StringBuilder sb, string title)
    {
        sb.AppendLine();
        sb.AppendLine(title);
        sb.AppendLine(new string('-', title.Length));
    }

    private static void Field(StringBuilder sb, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            sb.AppendLine($"{label}: {value}");
        }
    }

    private static bool HasAny(params string?[] values) => values.Any(v => !string.IsNullOrWhiteSpace(v));
}
