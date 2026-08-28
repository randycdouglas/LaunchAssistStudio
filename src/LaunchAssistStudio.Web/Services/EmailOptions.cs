namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Email settings. Secrets (SMTP password, Mailtrap API token) come from
/// appsettings.Production.json or environment variables - never from the
/// committed appsettings.json.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Which sender to use: <c>Mailtrap</c> or <c>Smtp</c>.</summary>
    public string Provider { get; set; } = EmailProviders.Smtp;

    // --- SMTP (MailKit) ---
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }

    // --- Mailtrap Email API ---
    public MailtrapOptions Mailtrap { get; set; } = new();

    // --- Shared ---
    public string FromAddress { get; set; } = "hello@launchassiststudio.com";
    public string FromName { get; set; } = "Launch Assist Studio";

    /// <summary>Where internal new-lead notifications are delivered.</summary>
    public string InternalNotificationAddress { get; set; } = "hello@launchassiststudio.com";

    public bool UsesMailtrap =>
        string.Equals(Provider, EmailProviders.Mailtrap, StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the selected provider has everything it needs to send.</summary>
    public bool IsConfigured => UsesMailtrap
        ? !string.IsNullOrWhiteSpace(Mailtrap.ApiToken)
        : !string.IsNullOrWhiteSpace(Host);
}

public class MailtrapOptions
{
    /// <summary>
    /// Mailtrap API token. Set in appsettings.Production.json (git-ignored) or
    /// via the Email__Mailtrap__ApiToken environment variable.
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>Groups messages in the Mailtrap dashboard.</summary>
    public string Category { get; set; } = "Launch Assist Studio";
}

public static class EmailProviders
{
    public const string Smtp = "Smtp";
    public const string Mailtrap = "Mailtrap";
}
