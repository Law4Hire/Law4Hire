using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Law4Hire.API.Controllers;

[Route("[controller]/[action]")]
public class CultureController : ControllerBase
{
    private readonly ILogger<CultureController> _logger;

    public CultureController(ILogger<CultureController> logger)
    {
        _logger = logger;
    }
    [HttpPost("SetCulture")]
    public IActionResult SetCulture(string culture, string redirectUri)
    {
        if (culture != null)
        {
            // This sets the language preference cookie in the user's browser.
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                // CORRECTED: Added IsEssential, SameSite=None, and Secure=true
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true, // Ensures the cookie is created
                    SameSite = SameSiteMode.None, // Allows cross-site cookie
                    Secure = true // Required when SameSite is None
                }
            );
        }

        // This redirects the user back to the page they were on.
        return LocalRedirect(redirectUri);
    }
}