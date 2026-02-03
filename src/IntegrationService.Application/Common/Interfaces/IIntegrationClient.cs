using IntegrationService.Domain.Common;

namespace IntegrationService.Application.Common.Interfaces;

/// <summary>
/// Interface for external integration client
/// </summary>
public interface IIntegrationClient
{
    /// <summary>
    /// Submits a new request to the external integration endpoint
    /// </summary>
    Task<Result<AddRequestResult>> AddRequestAsync(
        string requestId,
        string requestType,
        string requestData,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the external integration for request status and details
    /// </summary>
    Task<Result<InquiryResult>> InquireRequestAsync(
        string lookupId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from adding a request
/// </summary>
public class AddRequestResult
{
    public string ExternalRequestId { get; set; }
    public string Status { get; set; }
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// Result from inquiring about a request
/// </summary>
public class InquiryResult
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
