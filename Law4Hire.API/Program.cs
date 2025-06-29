using Law4Hire.API.Hubs;
using Law4Hire.Application.Services;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data;
using Law4Hire.Infrastructure.Data.Contexts;
using Law4Hire.Infrastructure.Data.Repositories;
using Law4Hire.Web.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Add services to the container ---

// Add Entity Framework DbContext
builder.Services.AddDbContext<Law4HireDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register repositories and services for dependency injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
builder.Services.AddScoped<IIntakeSessionRepository, IntakeSessionRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<ILocalizedContentRepository, LocalizedContentRepository>();
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IIntakeService, IntakeService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IFormIdentificationService, FormIdentificationService>();
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient()
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Law4Hire API", Version = "v1" });

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

var app = builder.Build();

// --- 2. Configure the HTTP request pipeline ---

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