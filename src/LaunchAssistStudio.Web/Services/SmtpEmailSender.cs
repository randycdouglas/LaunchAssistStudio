using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// SMTP transport, selected with <c>Email:Provider = "Smtp"</c>. Kept so the host's
/// own mailbox can be used instead of an API provider without a code change.
/// </summary>
public class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(
        string toAddress,
        string? toName,
        string subject,
        string textBody,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        var smtp = _options.Smtp;
        if (EmailOptions.IsUnset(smtp.Host))
        {
            throw new InvalidOperationException("Email:Smtp:Host is not configured.");
        }

        var message = new MimeMessage();
        // Always send From the authenticated mailbox so SPF/DKIM pass; the visitor
        // goes in Reply-To.
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(new MailboxAddress(toName ?? toAddress, toAddress));
        if (!string.IsNullOrWhiteSpace(replyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(replyTo));
        }

        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = textBody };

        using var client = new SmtpClient();
        var security = smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(smtp.Host!, smtp.Port, security, cancellationToken);

        if (!EmailOptions.IsUnset(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username!, smtp.Password ?? "", cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        logger.LogInformation("SMTP delivered \"{Subject}\" to {To}.", subject, toAddress);
    }
}
