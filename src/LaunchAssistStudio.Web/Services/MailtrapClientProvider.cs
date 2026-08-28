using Mailtrap;
using Microsoft.Extensions.Options;

namespace LaunchAssistStudio.Web.Services;

/// <summary>
/// Owns the single <see cref="MailtrapClientFactory"/> for the application.
/// The factory holds the underlying HttpClient, so it is created once and
/// reused rather than per message. Returns null when no token is configured
/// so a misconfigured environment degrades to "log and skip" instead of
/// throwing at startup.
/// </summary>
public sealed class MailtrapClientProvider(IOptions<EmailOptions> options) : IDisposable
{
    private readonly EmailOptions _options = options.Value;
    private readonly Lock _gate = new();

    private MailtrapClientFactory? _factory;
    private IMailtrapClient? _client;
    private bool _disposed;

    public IMailtrapClient? GetClient()
    {
        var token = _options.Mailtrap.ApiToken;
        if (string.IsNullOrWhiteSpace(token) || _disposed)
        {
            return null;
        }

        if (_client is not null)
        {
            return _client;
        }

        lock (_gate)
        {
            if (_client is null && !_disposed)
            {
                _factory = new MailtrapClientFactory(token);
                _client = _factory.CreateClient();
            }
        }

        return _client;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _factory?.Dispose();
            _factory = null;
            _client = null;
        }
    }
}
