using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Optional Cloudflare Turnstile check. Stays dormant until a secret key is
/// configured, so the form works out of the box and gains a CAPTCHA later
/// without a code change.
/// </summary>
public class TurnstileVerifier(
    HttpClient httpClient,
    IOptions<EmailOptions> options,
    ILogger<TurnstileVerifier> logger)
{
    private const string VerifyEndpoint = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<bool> IsHumanAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        var secret = options.Value.Turnstile.SecretKey;
        if (string.IsNullOrWhiteSpace(secret))
        {
            return true; // not configured — nothing to enforce
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Turnstile is enabled but the submission carried no token.");
            return false;
        }

        var form = new Dictionary<string, string> { ["secret"] = secret, ["response"] = token };
        if (!string.IsNullOrWhiteSpace(remoteIp)) form["remoteip"] = remoteIp;

        try
        {
            using var response = await httpClient.PostAsync(VerifyEndpoint, new FormUrlEncodedContent(form), cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<TurnstileResult>(cancellationToken);
            if (result?.Success != true)
            {
                logger.LogWarning("Turnstile rejected a submission: {Errors}",
                    result?.ErrorCodes is { Length: > 0 } e ? string.Join(", ", e) : "no detail");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // Fail closed: an unreachable verifier must not become an open door.
            logger.LogError(ex, "Turnstile verification call failed.");
            return false;
        }
    }

    private sealed class TurnstileResult
    {
        [JsonPropertyName("success")] public bool Success { get; init; }
        [JsonPropertyName("error-codes")] public string[]? ErrorCodes { get; init; }
    }
}
