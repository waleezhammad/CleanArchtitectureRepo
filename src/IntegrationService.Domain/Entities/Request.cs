using IntegrationService.Domain.Common;
using IntegrationService.Domain.Enums;

namespace IntegrationService.Domain.Entities;

/// <summary>
/// Represents a request submitted to the external integration
/// </summary>
public class Request : BaseEntity
{
    public string RequestId { get; private set; }
    public string ExternalRequestId { get; private set; }
    public string RequestType { get; private set; }
    public string RequestData { get; private set; }
    public RequestStatus Status { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string ResponseData { get; private set; }
    public string ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? LastRetryAt { get; private set; }

    private Request() { } // For EF Core

    public Request(string requestId, string requestType, string requestData)
    {
        RequestId = requestId;
        RequestType = requestType;
        RequestData = requestData;
        Status = RequestStatus.Pending;
        SubmittedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void MarkAsSubmitted(string externalRequestId)
    {
        ExternalRequestId = externalRequestId;
        Status = RequestStatus.Submitted;
    }

    public void MarkAsCompleted(string responseData)
    {
        Status = RequestStatus.Completed;
        ResponseData = responseData;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = RequestStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
        Status = RequestStatus.Retrying;
    }

    public bool CanRetry(int maxRetries)
    {
        return RetryCount < maxRetries && Status == RequestStatus.Failed;
    }
}
