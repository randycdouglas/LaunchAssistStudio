using System.Threading.RateLimiting;
using LaunchAssistStudio.Web.Models;
using LaunchAssistStudio.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Transports live behind IEmailSender and are chosen by configuration, so
// switching providers is a settings change rather than a code change.
builder.Services.AddScoped<SmtpEmailSender>();
builder.Services.AddHttpClient<ResendEmailSender>(client => client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IEmailSender>(sp =>
{
    var options = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
    return options.UsesSmtp
        ? sp.GetRequiredService<SmtpEmailSender>()
        : sp.GetRequiredService<ResendEmailSender>();
});

builder.Services.AddHttpClient<TurnstileVerifier>(client => client.Timeout = TimeSpan.FromSeconds(15));

// Anti-spam: throttle contact submissions per client IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("contact", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                // Generous enough that a person correcting mistakes on a long form is
                // never locked out, tight enough to stop bulk abuse.
                PermitLimit = 12,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
            }));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Extensionless URLs: /services -> /services/index.html. UseDefaultFiles only
// rewrites when the path already ends in a slash, so redirect directories first.
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (!string.IsNullOrEmpty(path) &&
        !path.EndsWith('/') &&
        !Path.HasExtension(path) &&
        !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
    {
        var candidate = Path.Combine(app.Environment.WebRootPath, path.Trim('/').Replace('/', Path.DirectorySeparatorChar), "index.html");
        if (File.Exists(candidate))
        {
            context.Request.Path = path + "/index.html";
        }
    }

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();

// Reports which transport is active and what is missing. Names only, never
// values, so it is safe to open in a browser.
app.MapGet("/api/health", (IOptions<EmailOptions> options) =>
{
    var email = options.Value;
    return Results.Ok(new
    {
        status = email.IsConfigured ? "ok" : "unconfigured",
        transport = email.Provider,
        from = email.FromAddress,
        deliversTo = email.InternalNotificationAddress,
        missingSettings = email.MissingSettings(),
        turnstile = string.IsNullOrWhiteSpace(email.Turnstile.SecretKey) ? "disabled" : "enabled",
    });
});

app.MapPost("/api/contact", async (
    ContactRequest request,
    HttpContext http,
    IEmailSender emailSender,
    TurnstileVerifier turnstile,
    IOptions<EmailOptions> options,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    // Honeypot: pretend success so bots get no signal to adapt to.
    if (!string.IsNullOrWhiteSpace(request.CompanyFax))
    {
        logger.LogWarning("Discarded suspected spam submission (honeypot filled).");
        return Results.Ok(new ContactResponse(true, "Thanks — we'll be in touch shortly."));
    }

    var errors = request.Validate();
    if (errors.Count > 0)
    {
        return Results.BadRequest(new ContactResponse(false, "Please check the highlighted fields.", errors));
    }

    if (!await turnstile.IsHumanAsync(request.TurnstileToken, http.Connection.RemoteIpAddress?.ToString(), cancellationToken))
    {
        return Results.BadRequest(new ContactResponse(false, "We couldn't verify that you're human. Please try again."));
    }

    var email = options.Value;
    var lead = request.ToLead();

    try
    {
        var (subject, body) = LeadEmailComposer.BuildInternalNotification(lead);
        await emailSender.SendAsync(email.InternalNotificationAddress, email.FromName, subject, body, lead.Email, cancellationToken);
    }
    catch (Exception ex)
    {
        // The mailbox is the system of record, so a failed notification is a lost
        // inquiry. Log the reason and tell the visitor how else to reach us.
        logger.LogError(ex, "Contact form: failed to deliver notification for {Email}.", lead.Email);
        return Results.Json(
            new ContactResponse(false, $"Sorry — we couldn't send your message. Please email {email.InternalNotificationAddress} directly."),
            statusCode: StatusCodes.Status502BadGateway);
    }

    try
    {
        var (subject, body) = LeadEmailComposer.BuildProspectAcknowledgement(lead);
        await emailSender.SendAsync(lead.Email, lead.ContactName, subject, body, replyTo: null, cancellationToken);
    }
    catch (Exception ex)
    {
        // The inquiry already reached us; the acknowledgement is a nicety.
        logger.LogError(ex, "Contact form: acknowledgement to {Email} failed.", lead.Email);
    }

    logger.LogInformation("Contact form: inquiry from {Email} delivered.", lead.Email);
    return Results.Ok(new ContactResponse(true, "Thanks — your project inquiry is on its way."));
})
.RequireRateLimiting("contact");

app.Run();
