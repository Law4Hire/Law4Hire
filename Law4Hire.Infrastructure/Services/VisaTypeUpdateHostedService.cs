using System.Text.Json;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Law4Hire.Infrastructure.Services;

public class VisaTypeUpdateHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<VisaTypeUpdateOptions> options,
    ILogger<VisaTypeUpdateHostedService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly VisaTypeUpdateOptions _options = options.Value;
    private readonly ILogger<VisaTypeUpdateHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // initial delay to align with schedule if needed
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateVisaTypesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visa types");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.UpdateIntervalMinutes), stoppingToken);
        }
    }

    private async Task UpdateVisaTypesAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.DataSource) || !File.Exists(_options.DataSource))
        {
            _logger.LogWarning("Visa type data source not found: {Path}", _options.DataSource);
            return;
        }

        using var stream = File.OpenRead(_options.DataSource);
        var visaTypes = await JsonSerializer.DeserializeAsync<List<VisaType>>(stream, cancellationToken: cancellationToken);
        if (visaTypes == null)
        {
            _logger.LogWarning("No visa types found in data source");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IVisaTypeRepository>();
        await repo.UpsertRangeAsync(visaTypes);
        _logger.LogInformation("Visa types updated: {Count}", visaTypes.Count);
    }
}
