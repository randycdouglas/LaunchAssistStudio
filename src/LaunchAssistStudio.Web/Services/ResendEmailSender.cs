using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Sends transactional mail through the Resend API over plain HTTPS - no SDK
/// dependency, so the project restores from nuget.org alone. Selected with
/// <c>Email:Provider = "Resend"</c>.
/// </summary>
public class ResendEmailSender(
    HttpClient httpClient,
    IOptions<EmailOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task SendAsync(string toAddress, string? toName, string subject, string textBody, CancellationToken cancellationToken = default)
    {
        var apiKey = _options.Resend.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Resend API key is not configured; skipping email \"{Subject}\" to {To}. " +
                              "Set Email:Resend:ApiKey in appsettings.Production.json or the " +
                              "Email__Resend__ApiKey environment variable.", subject, toAddress);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var payload = new ResendSendRequest
        {
            // Resend takes a single string, optionally as "Name <address>".
            From = FormatAddress(_options.FromAddress, _options.FromName),
            To = [toAddress],
            Subject = subject,
            Text = textBody,
            Tags = string.IsNullOrWhiteSpace(_options.Resend.Tag)
                ? null
                : [new ResendTag { Name = "category", Value = Sanitize(_options.Resend.Tag) }],
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Resend.SendEndpoint)
        {
            Content = JsonContent.Create(payload, options: SerializerOptions),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Never log the API key; the body carries Resend's error detail.
            throw new InvalidOperationException(
                $"Resend rejected the message with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
        }

        logger.LogInformation("Resend accepted \"{Subject}\" for {To} (id: {MessageId}). Logs: {LogUrl}",
            subject, toAddress, ReadId(body) ?? "none returned", "https://resend.com/emails");
    }

    private static string FormatAddress(string address, string? name) =>
        string.IsNullOrWhiteSpace(name) ? address : $"{Sanitize(name)} <{address}>";

    /// <summary>Strips characters that would break the "Name &lt;addr&gt;" header or a tag value.</summary>
    private static string Sanitize(string value) =>
        new(value.Where(c => c is not ('<' or '>' or '"' or '\r' or '\n' or ',')).ToArray());

    private static string? ReadId(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<ResendSendResponse>(body)?.Id;
        }
        catch (JsonException)
        {
            // A 2xx with an unexpected shape still means accepted; don't fail the send.
            return null;
        }
    }

    private sealed class ResendSendRequest
    {
        [JsonPropertyName("from")] public required string From { get; init; }
        [JsonPropertyName("to")] public required string[] To { get; init; }
        [JsonPropertyName("subject")] public required string Subject { get; init; }
        [JsonPropertyName("text")] public required string Text { get; init; }
        [JsonPropertyName("tags")] public ResendTag[]? Tags { get; init; }
    }

    private sealed class ResendTag
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("value")] public required string Value { get; init; }
    }

    private sealed class ResendSendResponse
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
    }
}
