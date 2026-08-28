namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Email settings. Secrets come from appsettings.Production.json on the server or
/// from Email__* environment variables - never from the committed appsettings.json.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Which transport to use: <c>Resend</c> (default) or <c>Smtp</c>.</summary>
    public string Provider { get; set; } = EmailProviders.Resend;

    public ResendOptions Resend { get; set; } = new();
    public SmtpOptions Smtp { get; set; } = new();
    public TurnstileOptions Turnstile { get; set; } = new();

    public string FromAddress { get; set; } = "hello@launchassiststudio.com";
    public string FromName { get; set; } = "Launch Assist Studio";

    /// <summary>Where new-inquiry notifications are delivered.</summary>
    public string InternalNotificationAddress { get; set; } = "hello@launchassiststudio.com";

    public bool UsesSmtp =>
        string.Equals(Provider, EmailProviders.Smtp, StringComparison.OrdinalIgnoreCase);

    public bool UsesResend =>
        string.Equals(Provider, EmailProviders.Resend, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// False when Provider is blank or a stale/typo'd value. Without this an
    /// unrecognised provider would quietly fall back to Resend while /api/health
    /// still reported the bogus name.
    /// </summary>
    public bool IsKnownProvider => UsesSmtp || UsesResend;

    /// <summary>The transport actually used, whatever Provider happens to say.</summary>
    public string ResolvedProvider => UsesSmtp ? EmailProviders.Smtp : EmailProviders.Resend;

    public bool IsConfigured => MissingSettings().Count == 0;

    /// <summary>
    /// Names (never values) of settings the active transport still needs.
    /// Shipped placeholders count as unset, so a half-finished config reports
    /// itself broken instead of appearing to work.
    /// </summary>
    public List<string> MissingSettings()
    {
        var missing = new List<string>();

        if (!IsKnownProvider) missing.Add("Email:Provider (unrecognised — use \"Resend\" or \"Smtp\")");

        if (IsUnset(FromAddress)) missing.Add("Email:FromAddress");
        if (IsUnset(InternalNotificationAddress)) missing.Add("Email:InternalNotificationAddress");

        if (UsesSmtp)
        {
            if (IsUnset(Smtp.Host)) missing.Add("Email:Smtp:Host");
            if (IsUnset(Smtp.Username)) missing.Add("Email:Smtp:Username");
            if (IsUnset(Smtp.Password)) missing.Add("Email:Smtp:Password");
        }
        else
        {
            if (IsUnset(Resend.ApiKey)) missing.Add("Email:Resend:ApiKey");
        }

        return missing;
    }

    /// <summary>Blank, or an obvious placeholder such as REPLACE_WITH_... / &lt;goes here&gt;.</summary>
    internal static bool IsUnset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;

        var v = value.Trim();
        return v.StartsWith("REPLACE_WITH", StringComparison.OrdinalIgnoreCase)
            || v.StartsWith('<')
            || v.Contains("goes here", StringComparison.OrdinalIgnoreCase)
            || v.Equals("changeme", StringComparison.OrdinalIgnoreCase);
    }
}

public class ResendOptions
{
    /// <summary>Resend API key ("re_..."). Never committed.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Tag applied to every message, for filtering in the Resend dashboard.</summary>
    public string Tag { get; set; } = "launch-assist-studio";

    public string SendEndpoint { get; set; } = "https://api.resend.com/emails";
}

public class SmtpOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class TurnstileOptions
{
    /// <summary>Public site key, rendered into the form when set.</summary>
    public string? SiteKey { get; set; }

    /// <summary>Server-side secret. Turnstile stays dormant until this is set.</summary>
    public string? SecretKey { get; set; }
}

public static class EmailProviders
{
    public const string Resend = "Resend";
    public const string Smtp = "Smtp";
}
