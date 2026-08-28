using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Sends transactional mail through the Mailtrap Email Sending API over plain
/// HTTPS - no SDK dependency, so the project restores from nuget.org alone.
/// Drop-in alternative to <see cref="SmtpEmailSender"/>, selected with
/// <c>Email:Provider = "Mailtrap"</c>.
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
            logger.LogWarning("Mailtrap API token is not configured; skipping email \"{Subject}\" to {To}. " +
                              "Set Email:Mailtrap:ApiToken in appsettings.Production.json or the " +
                              "Email__Mailtrap__ApiToken environment variable.", subject, toAddress);
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

        logger.LogInformation("Mailtrap accepted \"{Subject}\" for {To} (message ids: {MessageIds}). Logs: {LogUrl}",
            subject, toAddress,
            result?.MessageIds is { Length: > 0 } ids ? string.Join(", ", ids) : "none returned",
            "https://mailtrap.io/sending/email_logs");
    }

    private static MailtrapSendResponse? Deserialize(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<MailtrapSendResponse>(body);
        }
        catch (JsonException)
        {
            // A 2xx with an unexpected shape still means accepted; don't fail the send.
            return null;
        }
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
