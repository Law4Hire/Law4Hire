using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Law4Hire.Web.State;

public class CultureState
{
    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;
    public CultureInfo CurrentUICulture { get; private set; } = CultureInfo.CurrentUICulture;

    public event Action? OnChange;

    public void SetCultureFromCookie(IHttpContextAccessor httpContextAccessor)
    {
        var cookie = httpContextAccessor.HttpContext?.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        if (!string.IsNullOrWhiteSpace(cookie))
        {
            var requestCulture = CookieRequestCultureProvider.ParseCookieValue(cookie);
            if (requestCulture != null)
            {
                var culture = new CultureInfo(requestCulture.UICultures[0].Value ?? "en-US");
                CurrentCulture = culture;
                CurrentUICulture = culture;

                // This is crucial - set the thread cultures
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                NotifyStateChanged();
            }
        }
    }

    public void UpdateCulture(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CurrentCulture = culture;
        CurrentUICulture = culture;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}