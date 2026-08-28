namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Email settings. Secrets (SMTP password, Resend API key, Mailtrap API token)
/// come from appsettings.Production.json or environment variables - never from
/// the committed appsettings.json.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Which sender to use: <c>Resend</c>, <c>Mailtrap</c> or <c>Smtp</c>.</summary>
    public string Provider { get; set; } = EmailProviders.Resend;

    // --- SMTP (MailKit) ---
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }

    // --- Resend API ---
    public ResendOptions Resend { get; set; } = new();

    // --- Mailtrap Email API ---
    public MailtrapOptions Mailtrap { get; set; } = new();

    // --- Shared ---
    public string FromAddress { get; set; } = "hello@launchassiststudio.com";
    public string FromName { get; set; } = "Launch Assist Studio";

    /// <summary>Where internal new-lead notifications are delivered.</summary>
    public string InternalNotificationAddress { get; set; } = "hello@launchassiststudio.com";

    public bool UsesMailtrap =>
        string.Equals(Provider, EmailProviders.Mailtrap, StringComparison.OrdinalIgnoreCase);

    public bool UsesResend =>
        string.Equals(Provider, EmailProviders.Resend, StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the selected provider has everything it needs to send.</summary>
    public bool IsConfigured =>
        UsesResend ? !string.IsNullOrWhiteSpace(Resend.ApiKey)
        : UsesMailtrap ? !string.IsNullOrWhiteSpace(Mailtrap.ApiToken)
        : !string.IsNullOrWhiteSpace(Host);
}

public class ResendOptions
{
    /// <summary>
    /// Resend API key ("re_..."). Set in appsettings.Production.json (git-ignored)
    /// or via the Email__Resend__ApiKey environment variable.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>Tag applied to every message, for filtering in the Resend dashboard.</summary>
    public string Tag { get; set; } = "launch-assist-studio";

    public string SendEndpoint { get; set; } = "https://api.resend.com/emails";
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

    /// <summary>
    /// Live sending endpoint. Point at
    /// https://sandbox.api.mailtrap.io/api/send/{inbox_id} to capture mail in a
    /// Mailtrap sandbox inbox instead of delivering it.
    /// </summary>
    public string SendEndpoint { get; set; } = "https://send.api.mailtrap.io/api/send";
}

public static class EmailProviders
{
    public const string Smtp = "Smtp";
    public const string Mailtrap = "Mailtrap";
    public const string Resend = "Resend";
}
