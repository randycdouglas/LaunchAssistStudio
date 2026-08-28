namespace LaunchAssistStudio.Web.Services;

public interface IEmailSender
{
    /// <summary>
    /// Sends a plain-text message. <paramref name="replyTo"/> carries the visitor's
    /// address on notification mail: the message is sent From the authenticated
    /// sender so it passes SPF/DKIM, and replying still goes to the visitor.
    /// </summary>
    Task SendAsync(
        string toAddress,
        string? toName,
        string subject,
        string textBody,
        string? replyTo = null,
        CancellationToken cancellationToken = default);
}
