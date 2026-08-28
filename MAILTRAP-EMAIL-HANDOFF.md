# Mailtrap Transactional Email — Handoff for a New Site

**Purpose:** reuse the working Mailtrap email setup from Launch Assist Studio on another website.

This is a self-contained spec. You do not need access to the Launch Assist Studio repository — everything required is inline.

**Reference implementation:** https://github.com/randycdouglas/LaunchAssistStudio — see `src/LaunchAssistStudio.Web/Services/`.

---

## 1. Read this first: it is an API token, not SMTP

The owner may refer to this as "the SMTP info." Mailtrap offers two delivery routes and **this setup uses the API, not SMTP**:

| Route | What you need | Used here |
|---|---|---|
| **Email API** | one API token, sent as an HTTP header | ✅ yes |
| SMTP | host, port, username, password | ❌ no |

Ask the owner for the **API token** (Mailtrap → Settings → API Tokens). If they hand you SMTP host/username/password instead, that is the other route — see §8 for how to use it, but prefer the API token.

**The same Mailtrap account and token can serve multiple websites.** Set a different `Category` per site (§4) so messages are distinguishable in the Mailtrap dashboard.

---

## 2. Values the owner must supply

Do not invent these. Ask, and leave placeholders until you have them.

| Placeholder | Where to get it | Notes |
|---|---|---|
| `MAILTRAP_API_TOKEN` | Mailtrap → Settings → API Tokens | Needs send permission. **Secret.** |
| `FROM_ADDRESS` | e.g. `hello@thenewdomain.com` | Domain must be verified — see §7 |
| `FROM_NAME` | e.g. `The New Site` | Display name |
| `INTERNAL_NOTIFICATION_ADDRESS` | where form submissions go | Often the same as `FROM_ADDRESS` |
| `CATEGORY` | e.g. `The New Site` | Groups messages in the dashboard |

---

## 3. ⚠️ Secret handling — read before writing any config

The Launch Assist Studio repo is public, and `appsettings.Production.json` was **not** git-ignored by default. Putting a token there and committing would have published it. Verify the ignore rule **before** creating the file:

```bash
# add to .gitignore FIRST
appsettings.Production.json
appsettings.*.Local.json

# then confirm
git check-ignore -v src/YourApp/appsettings.Production.json
```

Commit an `appsettings.Production.json.example` with placeholders instead.

Environment variables are safer on shared hosting. ASP.NET Core maps `__` to `:`, so these work with no code change:

```
Email__Mailtrap__ApiToken=...
Email__Provider=Mailtrap
```

**Never** log the token, put it in a URL, or commit it. If it is ever exposed, rotate it in Mailtrap immediately.

---

## 4. Configuration shape

`appsettings.json` (committed — no secrets):

```json
{
  "Email": {
    "Provider": "Mailtrap",
    "Mailtrap": {
      "ApiToken": "",
      "Category": "CATEGORY",
      "SendEndpoint": "https://send.api.mailtrap.io/api/send"
    },
    "FromAddress": "FROM_ADDRESS",
    "FromName": "FROM_NAME",
    "InternalNotificationAddress": "INTERNAL_NOTIFICATION_ADDRESS"
  }
}
```

`appsettings.Production.json` (git-ignored — real values):

```json
{
  "Email": {
    "Mailtrap": { "ApiToken": "MAILTRAP_API_TOKEN" }
  }
}
```

---

## 5. .NET implementation (drop-in)

Verified on .NET 10 / ASP.NET Core. **No NuGet package required** — `HttpClient` and `System.Text.Json` only. Read §9 before considering the official SDK.

### 5.1 `Services/IEmailSender.cs`

```csharp
namespace YourApp.Services;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string? toName, string subject, string textBody, CancellationToken cancellationToken = default);
}
```

### 5.2 `Services/EmailOptions.cs`

```csharp
namespace YourApp.Services;

public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Which sender to use: Mailtrap or Smtp.</summary>
    public string Provider { get; set; } = "Mailtrap";

    public MailtrapOptions Mailtrap { get; set; } = new();

    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
    public string InternalNotificationAddress { get; set; } = "";

    public bool UsesMailtrap =>
        string.Equals(Provider, "Mailtrap", StringComparison.OrdinalIgnoreCase);
}

public class MailtrapOptions
{
    /// <summary>From appsettings.Production.json or Email__Mailtrap__ApiToken.</summary>
    public string? ApiToken { get; set; }

    /// <summary>Groups messages in the Mailtrap dashboard.</summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// Live endpoint. Point at https://sandbox.api.mailtrap.io/api/send/{inbox_id}
    /// to capture mail in a sandbox inbox instead of delivering it.
    /// </summary>
    public string SendEndpoint { get; set; } = "https://send.api.mailtrap.io/api/send";
}
```

### 5.3 `Services/MailtrapEmailSender.cs`

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace YourApp.Services;

/// <summary>
/// Sends transactional mail through the Mailtrap Email Sending API over plain
/// HTTPS - no SDK dependency, so the project restores from nuget.org alone.
/// </summary>
public class MailtrapEmailSender(
    HttpClient httpClient,
    IOptions<EmailOptions> options,
    ILogger<MailtrapEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task SendAsync(string toAddress, string? toName, string subject, string textBody, CancellationToken cancellationToken = default)
    {
        var token = _options.Mailtrap.ApiToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Mailtrap API token is not configured; skipping email \"{Subject}\" to {To}.", subject, toAddress);
            return;
        }

        var payload = new MailtrapSendRequest
        {
            From = new MailtrapAddress { Email = _options.FromAddress, Name = _options.FromName },
            To = [new MailtrapAddress { Email = toAddress, Name = string.IsNullOrWhiteSpace(toName) ? null : toName }],
            Subject = subject,
            Text = textBody,
            Category = _options.Mailtrap.Category,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Mailtrap.SendEndpoint)
        {
            Content = JsonContent.Create(payload, options: SerializerOptions),
        };
        request.Headers.Add("Api-Token", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Never log the token; the body carries Mailtrap's error detail.
            throw new InvalidOperationException(
                $"Mailtrap rejected the message with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
        }

        var result = Deserialize(body);
        if (result is { Success: false })
        {
            throw new InvalidOperationException(
                $"Mailtrap reported failure: {string.Join("; ", result.Errors ?? ["no detail returned"])}");
        }

        logger.LogInformation("Mailtrap accepted \"{Subject}\" for {To} (message ids: {MessageIds}).",
            subject, toAddress,
            result?.MessageIds is { Length: > 0 } ids ? string.Join(", ", ids) : "none returned");
    }

    private static MailtrapSendResponse? Deserialize(string body)
    {
        try { return JsonSerializer.Deserialize<MailtrapSendResponse>(body); }
        catch (JsonException) { return null; }  // 2xx with odd shape still means accepted
    }

    private sealed class MailtrapSendRequest
    {
        [JsonPropertyName("from")] public required MailtrapAddress From { get; init; }
        [JsonPropertyName("to")] public required MailtrapAddress[] To { get; init; }
        [JsonPropertyName("subject")] public required string Subject { get; init; }
        [JsonPropertyName("text")] public required string Text { get; init; }
        [JsonPropertyName("category")] public string? Category { get; init; }
    }

    private sealed class MailtrapAddress
    {
        [JsonPropertyName("email")] public required string Email { get; init; }
        [JsonPropertyName("name")] public string? Name { get; init; }
    }

    private sealed class MailtrapSendResponse
    {
        [JsonPropertyName("success")] public bool Success { get; init; } = true;
        [JsonPropertyName("message_ids")] public string[]? MessageIds { get; init; }
        [JsonPropertyName("errors")] public string[]? Errors { get; init; }
    }
}
```

### 5.4 `Program.cs` registration

```csharp
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services.AddHttpClient<MailtrapEmailSender>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IEmailSender>(sp => sp.GetRequiredService<MailtrapEmailSender>());
```

Use `AddHttpClient` (not `new HttpClient()`) so the handler is pooled and DNS changes are picked up.

### 5.5 Calling it — do not let email loss become data loss

Save the record **first**, then send, and never let a send failure fail the request:

```csharp
db.Leads.Add(lead);
await db.SaveChangesAsync(cancellationToken);   // durable first

try
{
    await emailSender.SendAsync(options.Value.InternalNotificationAddress, "Site", subject, body, cancellationToken);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to send notification for {Id}.", lead.PublicId);
}
```

This was verified: with the API returning `401`, the submission still persisted.

---

## 6. Any other stack — the raw contract

```http
POST https://send.api.mailtrap.io/api/send
Api-Token: MAILTRAP_API_TOKEN
Content-Type: application/json

{
  "from":     { "email": "FROM_ADDRESS", "name": "FROM_NAME" },
  "to":       [ { "email": "recipient@example.com", "name": "Recipient" } ],
  "subject":  "Subject line",
  "text":     "Plain text body",
  "category": "CATEGORY"
}
```

Success → `200` with `{"success":true,"message_ids":["..."]}`
Failure → `4xx/5xx` with `{"success":false,"errors":["..."]}`

Optional fields: `html`, `cc`, `bcc`, `reply_to`, `attachments`, `headers`, `custom_variables`, or `template_uuid` + `template_variables` for Mailtrap-hosted templates.

Auth alternative: `Authorization: Bearer <token>`.

---

## 7. Sending domain — do this before going live

Mailtrap will not deliver from an unverified domain.

1. Mailtrap → Sending Domains → add the new site's domain.
2. Add the SPF, DKIM and DMARC DNS records at the registrar.
3. Wait for verification to pass.

Until verified, use one of:

- **Sandbox** — set `SendEndpoint` to `https://sandbox.api.mailtrap.io/api/send/{inbox_id}`. Mail is captured, never delivered. Best for development.
- **Demo domain** — set `FromAddress` to `hello@demomailtrap.co`. Works immediately but **only delivers to the Mailtrap account owner's own address**, so it is useless for testing real recipients.

Verify a domain per site, or use one verified domain and set `FromAddress` to a subdomain/alias you control.

---

## 8. If you must use SMTP instead

Mailtrap also exposes SMTP (`smtp.mailtrap.io`, port 587, STARTTLS, username + password). Implement the same `IEmailSender` with MailKit:

```csharp
// dotnet add package MailKit
using var client = new SmtpClient();
await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
await client.AuthenticateAsync(username, password, ct);
await client.SendAsync(message, ct);
await client.DisconnectAsync(true, ct);
```

Keep it behind the same interface so `Email:Provider` switches between them. The API route is simpler and is the default.

---

## 9. Traps already hit — do not repeat these

1. **The `Mailtrap` package on nuget.org is not Mailtrap's.** It is published by an unrelated account (`flaviodamaiajr`, ~6k downloads). Do not install it.

2. **The official SDK is behind an authenticated feed.** It lives on Mailtrap's GitHub Packages registry and needs a GitHub PAT on *every* machine that builds or deploys — otherwise `dotnet restore` fails with `401`. This was tried and then removed in favour of the direct HTTP call above. Avoid unless there is a strong reason.

3. **`dotnet nuget add source` writes to the *nearest* `nuget.config`.** Run from a repo folder, it edits the repo's file — so `--store-password-in-clear-text` would commit a PAT into the repository. This was reproduced and confirmed. If you ever must run it, target the user config explicitly:
   `--configfile "$env:APPDATA\NuGet\NuGet.Config"`

4. **`demomailtrap.co` only reaches the account owner.** It looks like it works, then silently fails for everyone else.

5. **Email failure must not roll back the record.** Persist first, wrap the send in try/catch.

---

## 10. How to test without a real token

A local stand-in proves the endpoint, header and JSON body are correct without sending anything. Point `Email:Mailtrap:SendEndpoint` at `http://localhost:5199/api/send` in `appsettings.Development.json`, set any dummy token, and run this PowerShell listener:

```powershell
$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add("http://localhost:5199/")
$listener.Start()
"listening..."
$ctx  = $listener.GetContext()
$rq   = $ctx.Request
$body = (New-Object System.IO.StreamReader($rq.InputStream, $rq.ContentEncoding)).ReadToEnd()
"Api-Token: $($rq.Headers['Api-Token'])"
"Body: $body"
# reply like the real API (use 401 + success:false to test the failure path)
$payload = '{"success":true,"message_ids":["test-id"]}'
$bytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
$ctx.Response.StatusCode = 200
$ctx.Response.ContentType = "application/json"
$ctx.Response.OutputStream.Write($bytes, 0, $bytes.Length)
$ctx.Response.Close()
$listener.Stop()
```

Check: `POST`, `Content-Type: application/json`, `Api-Token` header present, and a body with `from` / `to` / `subject` / `text` / `category`. Then rerun returning `401` to confirm the record still saves.

**Remember to revert the dev endpoint and delete test records afterwards.**

---

## 11. Checking real deliveries

**https://mailtrap.io/sending/email_logs** — every sent message, its status, and bounce/spam detail. Filter by the `Category` you set for this site.

---

## 12. Definition of done

- [ ] API token supplied by the owner and stored **outside source control**
- [ ] `.gitignore` covers `appsettings.Production.json`, verified with `git check-ignore`
- [ ] Sending domain verified in Mailtrap (or sandbox endpoint in use)
- [ ] `Category` set to something unique to this site
- [ ] Send succeeds; message appears in the Mailtrap email logs
- [ ] Failure path tested: record still saves when the API returns an error
- [ ] No token in the repo, in logs, or in any URL
