using Law4Hire.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container for the Blazor Web App.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===================================================================================
// ** FIX: Register HttpClient Service **
// This is the crucial part that was missing. An HttpClient must be registered in the 
// service container so that Blazor pages can use `@inject` to make API calls.
// This is the standard and correct way to do this in .NET 9.
// ===================================================================================
builder.Services.AddScoped(sp => new HttpClient
{
    // This URL must point to your running backend API project.
    // You can find the correct port in your Law4Hire.API project's 
    // Properties/launchSettings.json file.
    // A common default is "https://localhost:7123" or similar.
    BaseAddress = new Uri("https://localhost:7123")
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();