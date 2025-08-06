using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<Law4HireDbContext>(options =>
            options.UseSqlServer(
                context.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    sqlOptions.MigrationsAssembly("Law4Hire.Infrastructure");
                }));

        services.AddHttpClient("OpenAI", client =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OpenAIKey") ?? string.Empty;
            client.BaseAddress = new Uri("https://api.openai.com/v1/chat/completions");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddLogging(builder => builder.AddConsole());
    })
    .Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();
var dbContext = services.GetRequiredService<Law4HireDbContext>();

await dbContext.Database.MigrateAsync();

var client = services.GetRequiredService<IHttpClientFactory>().CreateClient("OpenAI");
var countries = await dbContext.Countries.ToListAsync();
var visaTypes = await dbContext.BaseVisaTypes.ToListAsync();

foreach (var country in countries)
{
    foreach (var visaType in visaTypes)
    {
        if (await dbContext.VisaWorkflows.AnyAsync(v => v.CountryId == country.Id && v.VisaTypeId == visaType.Id))
        {
            logger.LogInformation("Skipping existing workflow for {Country}-{VisaType}", country.Name, visaType.Code);
            continue;
        }

        logger.LogInformation("Processing workflow for {Country}-{VisaType}", country.Name, visaType.Code);

        var systemMessage = new
        {
            role = "system",
            content = "You are a U.S. immigration expert. Given a visa type and a country of citizenship, return a step-by-step workflow in JSON. Include name, description, government forms, estimated cost (or null), URLs, required documents, medical requirements, and any consulate-specific instructions. Do not guess. If the information is unavailable, say so clearly in the response and mark the status as 'incomplete'."
        };

        var userPayload = new
        {
            visa_type = visaType.Code,
            country = country.Name,
            citizenship_country_code = country.CountryCode2,
            format = "json"
        };
        var userMessage = new { role = "user", content = JsonSerializer.Serialize(userPayload) };
        var chatRequest = new { model = "gpt-4", messages = new[] { systemMessage, userMessage } };

        var requestJson = JsonSerializer.Serialize(chatRequest);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await client.PostAsync(string.Empty, content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseJson);
                var messageContent = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()!;

                using var workflowDoc = JsonDocument.Parse(messageContent);
                var status = workflowDoc.RootElement.GetProperty("status").GetString();

                if (status == "complete")
                {
                    var workflow = new VisaWorkflow
                    {
                        Id = Guid.NewGuid(),
                        CountryId = country.Id,
                        VisaTypeId = visaType.Id,
                        WorkflowJson = messageContent,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    dbContext.VisaWorkflows.Add(workflow);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Saved workflow for {Country}-{VisaType}", country.Name, visaType.Code);
                }
                else
                {
                    logger.LogWarning("Incomplete workflow for {Country}-{VisaType}", country.Name, visaType.Code);
                }
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {Country}-{VisaType}, attempt {Attempt}", country.Name, visaType.Code, attempt);
                if (attempt == maxRetries)
                {
                    logger.LogError("Failed to get workflow for {Country}-{VisaType} after {MaxRetries} attempts.", country.Name, visaType.Code, maxRetries);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                }
            }
        }
    }
}