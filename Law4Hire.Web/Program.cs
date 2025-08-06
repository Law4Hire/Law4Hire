using Law4Hire.Web.Components;
using Law4Hire.Web.State;
using Law4Hire.Application.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Microsoft.FluentUI.AspNetCore.Components;
using MudBlazor.Services;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Law4Hire.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Fluent UI services
builder.Services.AddFluentUIComponents();

builder.Services.AddScoped<CultureState>();
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<VisaNarrowingService>();
builder.Services.AddScoped<VisaEligibilityService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<VisaInterviewBot>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var context = sp.GetRequiredService<Law4HireDbContext>();
    return new VisaInterviewBot(httpClient, "not-needed", configuration, context);
});
builder.Services.AddScoped<WorkflowProcessingService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add database context
builder.Services.AddDbContext<Law4HireDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Configure password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Configure username requirements  
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<Law4HireDbContext>()
.AddDefaultTokenProviders();

// ✅ Add this
builder.Services.AddControllers();

// Configure HttpClient to use API project endpoints
builder.Services.AddHttpClient<HttpClient>(client =>
{
    // Point to the separate API project
    client.BaseAddress = new Uri("http://localhost:5237");
});

// Also add the scoped HttpClient for injection
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(typeof(HttpClient).Name));

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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ✅ Add this
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Make Program accessible for testing
namespace Law4Hire.Web
{
    public partial class Program { }
}