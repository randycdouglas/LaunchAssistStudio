using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaunchAssistStudio.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public int? ErrorStatusCode { get; private set; }

    public void OnGet(int? statusCode)
    {
        ErrorStatusCode = statusCode;
    }
}
