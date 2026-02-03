using IntegrationService.Domain.Common;
using MediatR;

namespace IntegrationService.Application.Requests.Queries.InquireRequest;

/// <summary>
/// Query to inquire about a request status from the external integration
/// </summary>
public record InquireRequestQuery : IRequest<Result<InquireRequestResponse>>
{
    public string RequestId { get; init; }
    public string ExternalRequestId { get; init; }
}

/// <summary>
/// Response for inquiry query
/// </summary>
public record InquireRequestResponse
{
    public string RequestId { get; init; }
    public string ExternalRequestId { get; init; }
    public string Status { get; init; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string ResponseData { get; init; }
    public string ErrorMessage { get; init; }
    public Dictionary<string, object> AdditionalInfo { get; init; }
}
