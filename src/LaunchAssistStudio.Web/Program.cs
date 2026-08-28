using System.Threading.RateLimiting;
using LaunchAssistStudio.Web.Data;
using LaunchAssistStudio.Web.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Email provider is selected by configuration (Email:Provider = Mailtrap | Smtp).
// The Mailtrap client factory owns an HttpClient, so it is a singleton.
builder.Services.AddSingleton<MailtrapClientProvider>();
builder.Services.AddScoped<SmtpEmailSender>();
builder.Services.AddScoped<MailtrapEmailSender>();
builder.Services.AddScoped<IEmailSender>(sp =>
{
    var emailOptions = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
    return emailOptions.UsesMailtrap
        ? sp.GetRequiredService<MailtrapEmailSender>()
        : sp.GetRequiredService<SmtpEmailSender>();
});

// Anti-spam: throttle intake form submissions (POST /start-project) per client IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        if (HttpMethods.IsPost(httpContext.Request.Method) &&
            httpContext.Request.Path.StartsWithSegments("/start-project"))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                "lead-form:" + (httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(10),
                    QueueLimit = 0,
                });
        }

        return RateLimitPartition.GetNoLimiter("no-limit");
    });
});

var app = builder.Build();

// Apply pending EF Core migrations at startup (disable via Database:ApplyMigrationsAtStartup).
if (app.Configuration.GetValue("Database:ApplyMigrationsAtStartup", defaultValue: true))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.MapRazorPages();

app.Run();
