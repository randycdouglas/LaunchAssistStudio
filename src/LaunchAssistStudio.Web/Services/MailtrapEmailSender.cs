using Mailtrap.Emails.Requests;
using Mailtrap.Emails.Responses;
using Microsoft.Extensions.Options;

namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Sends transactional mail through the Mailtrap Email API. Drop-in
/// alternative to <see cref="SmtpEmailSender"/>; selected with
/// <c>Email:Provider = "Mailtrap"</c>.
/// </summary>
public class MailtrapEmailSender(
    MailtrapClientProvider clientProvider,
    IOptions<EmailOptions> options,
    ILogger<MailtrapEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string toAddress, string? toName, string subject, string textBody, CancellationToken cancellationToken = default)
    {
        var client = clientProvider.GetClient();
        if (client is null)
        {
            logger.LogWarning("Mailtrap API token is not configured; skipping email \"{Subject}\" to {To}. " +
                              "Set Email:Mailtrap:ApiToken in appsettings.Production.json or the " +
                              "Email__Mailtrap__ApiToken environment variable.", subject, toAddress);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var request = SendEmailRequest
            .Create()
            .From(_options.FromAddress, _options.FromName)
            .To(toAddress)
            .Subject(subject)
            .Category(_options.Mailtrap.Category)
            .Text(textBody);

        SendEmailResponse? response = await client.Email().Send(request);

        logger.LogInformation("Mailtrap accepted \"{Subject}\" for {To}. Delivery log: {LogUrl}",
            subject, toAddress, "https://mailtrap.io/sending/email_logs");
    }
}
