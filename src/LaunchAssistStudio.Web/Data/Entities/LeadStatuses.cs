namespace LaunchAssistStudio.Web.Data.Entities;

/// <summary>
/// Well-known lead statuses. Stored as strings so new pipeline stages can be
/// added without a schema migration.
/// </summary>
public static class LeadStatuses
{
    public const string NewLead = "New Lead";
    public const string Contacted = "Contacted";
    public const string Qualified = "Qualified";
    public const string ProposalSent = "Proposal Sent";
    public const string Won = "Won";
    public const string Lost = "Lost";
}
