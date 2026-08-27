namespace LaunchAssistStudio.Web.Services;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string? toName, string subject, string textBody, CancellationToken cancellationToken = default);
}
