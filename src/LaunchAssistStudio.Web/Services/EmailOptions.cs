namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// SMTP settings. Host/Username/Password are supplied via user secrets or
/// environment variables — never committed to source control.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string FromAddress { get; set; } = "hello@launchassiststudio.com";
    public string FromName { get; set; } = "Launch Assist Studio";

    /// <summary>Where internal new-lead notifications are delivered.</summary>
    public string InternalNotificationAddress { get; set; } = "hello@launchassiststudio.com";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
