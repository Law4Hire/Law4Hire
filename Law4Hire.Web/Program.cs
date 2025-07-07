using Law4Hire.Web.Components;
using Law4Hire.Web.State;
using Microsoft.AspNetCore.Localization;
using System.Globalization; // Add this using statement

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CultureState>();
builder.Services.AddScoped<AuthState>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// ✅ Add this
builder.Services.AddControllers();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7280")
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

var supportedCultures = new[]
{
    "en-US", "es-ES", "zh-CN", "hi-IN", "ar-SA", "bn-BD", "pt-PT", "ru-RU", "ja-JP", "de-DE", "fr-FR",
    "ur-PK", "id-ID", "tr-TR", "it-IT", "vi-VN", "ko-KR", "ta-IN", "te-IN", "mr-IN", "pl-PL"
}.Select(c => new CultureInfo(c)).ToList();

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures.Select(c => c.Name).ToArray())
    .AddSupportedUICultures(supportedCultures.Select(c => c.Name).ToArray());

localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ✅ Add this
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();