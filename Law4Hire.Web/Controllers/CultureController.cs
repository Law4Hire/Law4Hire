using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Law4Hire.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class CultureController : Controller
    {
        [HttpGet]
        public IActionResult SetCulture(string culture, string redirectUri = "/")
        {
            // Use Url helper, not Request
            if (!Url.IsLocalUrl(redirectUri))
                redirectUri = "/";

            var cultureCookie = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                cultureCookie,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1), // Persist 1 year
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    HttpOnly = false
                }
            );
            return LocalRedirect(redirectUri);
        }
    }
}
