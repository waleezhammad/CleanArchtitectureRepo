using IntegrationService.Application.Common.Interfaces;
using IntegrationService.Domain.Common;
using IntegrationService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationService.Infrastructure.ExternalServices;

/// <summary>
/// Implementation of the external integration client
/// </summary>
public class IntegrationClient : IIntegrationClient
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationSettings _settings;
    private readonly ILogger<IntegrationClient> _logger;

    public IntegrationClient(
        HttpClient httpClient,
        IOptions<IntegrationSettings> settings,
        ILogger<IntegrationClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<AddRequestResult>> AddRequestAsync(
        string requestId,
        string requestType,
        string requestData,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting request {RequestId} to external integration", requestId);

            var payload = new
            {
                requestId,
                requestType,
                data = requestData,
                metadata,
                timestamp = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync(
                _settings.AddRequestEndpoint,
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to submit request. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, 
                    errorContent);
                
                return Result.Failure<AddRequestResult>(
                    $"External API returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<ExternalAddResponse>(cancellationToken);
            
            if (result == null)
            {
                return Result.Failure<AddRequestResult>("Failed to parse response from external API");
            }

            _logger.LogInformation(
                "Successfully submitted request {RequestId}, received external ID {ExternalRequestId}", 
                requestId, 
                result.ExternalRequestId);

            return Result.Success(new AddRequestResult
            {
                ExternalRequestId = result.ExternalRequestId,
                Status = result.Status,
                SubmittedAt = result.SubmittedAt
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while submitting request");
            return Result.Failure<AddRequestResult>($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while submitting request");
            return Result.Failure<AddRequestResult>($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<InquiryResult>> InquireRequestAsync(
        string lookupId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Inquiring request status for {LookupId}", lookupId);

            var url = $"{_settings.InquiryEndpoint}?requestId={Uri.EscapeDataString(lookupId)}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to inquire request. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, 
                    errorContent);
                
                return Result.Failure<InquiryResult>(
                    $"External API returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<ExternalInquiryResponse>(cancellationToken);
            
            if (result == null)
            {
                return Result.Failure<InquiryResult>("Failed to parse response from external API");
            }

            _logger.LogInformation(
                "Successfully retrieved inquiry data for {LookupId}, Status: {Status}", 
                lookupId, 
                result.Status);

            return Result.Success(new InquiryResult
            {
                RequestId = result.RequestId,
                ExternalRequestId = result.ExternalRequestId,
                Status = result.Status,
                SubmittedAt = result.SubmittedAt,
                CompletedAt = result.CompletedAt,
                ResponseData = result.ResponseData,
                ErrorMessage = result.ErrorMessage,
                AdditionalInfo = result.AdditionalInfo ?? new Dictionary<string, object>()
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while inquiring request");
            return Result.Failure<InquiryResult>($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while inquiring request");
            return Result.Failure<InquiryResult>($"Unexpected error: {ex.Message}");
        }
    }

    // DTOs for external API communication
    private class ExternalAddResponse
    {
        public string ExternalRequestId { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    private class ExternalInquiryResponse
    {
        public string RequestId { get; set; }
        public string ExternalRequestId { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ResponseData { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }
    }
}
