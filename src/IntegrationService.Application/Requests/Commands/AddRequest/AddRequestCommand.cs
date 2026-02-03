using IntegrationService.Domain.Common;
using MediatR;

namespace IntegrationService.Application.Requests.Commands.AddRequest;

/// <summary>
/// Command to add a new request to the external integration
/// </summary>
public record AddRequestCommand : IRequest<Result<AddRequestResponse>>
{
    public string RequestType { get; init; }
    public string RequestData { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
}

/// <summary>
/// Response for add request command
/// </summary>
public record AddRequestResponse
{
    public string RequestId { get; init; }
    public string ExternalRequestId { get; init; }
    public string Status { get; init; }
    public DateTime SubmittedAt { get; init; }
}
