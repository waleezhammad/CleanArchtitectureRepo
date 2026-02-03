namespace IntegrationService.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for external integration
/// </summary>
public class IntegrationSettings
{
    public const string SectionName = "IntegrationSettings";

    public string BaseUrl { get; set; }
    public string AddRequestEndpoint { get; set; }
    public string InquiryEndpoint { get; set; }
    public string ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool EnableRetry { get; set; } = true;
}
