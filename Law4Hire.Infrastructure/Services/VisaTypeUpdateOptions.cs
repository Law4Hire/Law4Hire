namespace Law4Hire.Infrastructure.Services;

public class VisaTypeUpdateOptions
{
    /// <summary>
    /// Interval in minutes between update runs.
    /// </summary>
    public int UpdateIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Path to the JSON file containing visa type data.
    /// </summary>
    public string DataSource { get; set; } = string.Empty;
}
