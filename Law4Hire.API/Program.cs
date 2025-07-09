using Law4Hire.API.Hubs;
using Law4Hire.Application.Services;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data;
using Law4Hire.Infrastructure.Data.Contexts;
using Law4Hire.Infrastructure.Data.Initialization;
using Law4Hire.Infrastructure.Data.Repositories;
using Law4Hire.Web.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

builder.Services.AddDbContext<Law4HireDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);

            sqlOptions.MigrationsAssembly("Law4Hire.API");
        });
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
    }
});

// Register repositories and services for dependency injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
builder.Services.AddScoped<IIntakeSessionRepository, IntakeSessionRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<ILocalizedContentRepository, LocalizedContentRepository>();
builder.Services.AddScoped<IVisaTypeRepository, VisaTypeRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IIntakeService, IntakeService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IFormIdentificationService, FormIdentificationService>();
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<Law4HireDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddHttpClient("Law4Hire.API", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7123");
//});



//builder.Services.AddScoped(sp =>
//{
//    var factory = sp.GetRequiredService<IHttpClientFactory>();
//    return factory.CreateClient("Law4Hire.API");
//});

// ** OpenAPI (Swagger) Configuration **
// This section configures the services needed to generate the OpenAPI specification document.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Law4Hire Immigration Legal API",
        Version = "v1",
        Description = "API for managing client registrations, visa intake interviews, service packages, and case processing."
    });

    // Define the Bearer token security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSignalR();
builder.Services.AddHealthChecks().AddDbContextCheck<Law4HireDbContext>();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromSeconds(10);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 50;
    });
});

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("es-ES"),
    new CultureInfo("fr-FR"),
    new CultureInfo("zh-CN"),
    new CultureInfo("hi-IN"),
    new CultureInfo("ar-SA"),
    new CultureInfo("bn-BD"),
    new CultureInfo("pt-PT"),
    new CultureInfo("ru-RU"),
    new CultureInfo("ja-JP"),
    new CultureInfo("de-DE"),
    new CultureInfo("ur-PK"),
    new CultureInfo("id-ID"),
    new CultureInfo("tr-TR"),
    new CultureInfo("it-IT"),
    new CultureInfo("vi-VN"),
    new CultureInfo("ko-KR"),
    new CultureInfo("ta-IN"),
    new CultureInfo("te-IN"),
    new CultureInfo("mr-IN"),
    new CultureInfo("pl-PL") 
};
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AIOnly", policy => policy.RequireRole("AI"));
    options.AddPolicy("LegalProfessional", policy => policy.RequireRole("LegalProfessional"));
});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedAsync(services);
}
if (app.Environment.IsDevelopment())
{
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseHsts();

// The UseSwagger middleware serves the generated OpenAPI/JSON file.
app.UseSwagger();
// The UseSwaggerUI middleware serves the Swagger UI, which uses the JSON file to render the interactive API documentation.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Law4Hire API v1");
    // Serve the UI at the app's root
    c.RoutePrefix = string.Empty;
});
    app.UseDeveloperExceptionPage();
// Seed the database in development
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<Law4HireDbContext>();
// Correct way to get a logger for a static class
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("DataSeeder");
await context.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(context, logger);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRequestLocalization();
app.UseHttpLogging(); // Put this after `app.UseRouting()` and before `app.UseAuthorization()`

// Enable logging options (if needed)


// Use global rate limiting
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<IntakeChatHub>("/hubs/intake");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
.AddInteractiveServerRenderMode();
app.Run();