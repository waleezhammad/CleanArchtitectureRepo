using IntegrationService.Application.Common.Interfaces;
using IntegrationService.Domain.Common;
using IntegrationService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntegrationService.Application.Requests.Commands.AddRequest;

/// <summary>
/// Handles the AddRequestCommand
/// </summary>
public class AddRequestCommandHandler : IRequestHandler<AddRequestCommand, Result<AddRequestResponse>>
{
    private readonly IIntegrationClient _integrationClient;
    private readonly IRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddRequestCommandHandler> _logger;

    public AddRequestCommandHandler(
        IIntegrationClient integrationClient,
        IRequestRepository requestRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddRequestCommandHandler> logger)
    {
        _integrationClient = integrationClient;
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AddRequestResponse>> Handle(
        AddRequestCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing add request command for type: {RequestType}", request.RequestType);

            // Generate unique request ID
            var requestId = GenerateRequestId();

            // Create domain entity
            var requestEntity = new Request(requestId, request.RequestType, request.RequestData);

            // Save to local database (tracking)
            await _requestRepository.AddAsync(requestEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Submit to external integration
            var submissionResult = await _integrationClient.AddRequestAsync(
                requestId,
                request.RequestType,
                request.RequestData,
                request.Metadata,
                cancellationToken);

            if (submissionResult.IsFailure)
            {
                _logger.LogError("Failed to submit request to external system: {Error}", submissionResult.Error);
                requestEntity.MarkAsFailed(submissionResult.Error);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Failure<AddRequestResponse>(submissionResult.Error);
            }

            // Update entity with external request ID
            requestEntity.MarkAsSubmitted(submissionResult.Value.ExternalRequestId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully submitted request {RequestId} with external ID {ExternalRequestId}", 
                requestId, 
                submissionResult.Value.ExternalRequestId);

            var response = new AddRequestResponse
            {
                RequestId = requestId,
                ExternalRequestId = submissionResult.Value.ExternalRequestId,
                Status = requestEntity.Status.ToString(),
                SubmittedAt = requestEntity.SubmittedAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing add request command");
            return Result.Failure<AddRequestResponse>($"Internal error: {ex.Message}");
        }
    }

    private string GenerateRequestId()
    {
        return $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 50);
    }
}
