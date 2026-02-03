using IntegrationService.Application.Common.Interfaces;
using IntegrationService.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntegrationService.Application.Requests.Queries.InquireRequest;

/// <summary>
/// Handles the InquireRequestQuery
/// </summary>
public class InquireRequestQueryHandler : IRequestHandler<InquireRequestQuery, Result<InquireRequestResponse>>
{
    private readonly IIntegrationClient _integrationClient;
    private readonly IRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InquireRequestQueryHandler> _logger;

    public InquireRequestQueryHandler(
        IIntegrationClient integrationClient,
        IRequestRepository requestRepository,
        IUnitOfWork unitOfWork,
        ILogger<InquireRequestQueryHandler> logger)
    {
        _integrationClient = integrationClient;
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<InquireRequestResponse>> Handle(
        InquireRequestQuery query, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing inquiry for RequestId: {RequestId}, ExternalRequestId: {ExternalRequestId}", 
                query.RequestId, 
                query.ExternalRequestId);

            // Determine which ID to use for lookup
            string lookupId = !string.IsNullOrEmpty(query.ExternalRequestId) 
                ? query.ExternalRequestId 
                : query.RequestId;

            if (string.IsNullOrEmpty(lookupId))
            {
                return Result.Failure<InquireRequestResponse>("Either RequestId or ExternalRequestId must be provided");
            }

            // Query the external integration
            var inquiryResult = await _integrationClient.InquireRequestAsync(
                lookupId, 
                cancellationToken);

            if (inquiryResult.IsFailure)
            {
                _logger.LogError("Failed to inquire request from external system: {Error}", inquiryResult.Error);
                return Result.Failure<InquireRequestResponse>(inquiryResult.Error);
            }

            var externalData = inquiryResult.Value;

            // Try to find and update local record if exists
            if (!string.IsNullOrEmpty(query.RequestId))
            {
                var localRequest = await _requestRepository.GetByRequestIdAsync(query.RequestId, cancellationToken);
                if (localRequest != null)
                {
                    // Update local record with latest status from external system
                    if (externalData.Status == "Completed" && localRequest.Status != Domain.Enums.RequestStatus.Completed)
                    {
                        localRequest.MarkAsCompleted(externalData.ResponseData);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else if (externalData.Status == "Failed" && localRequest.Status != Domain.Enums.RequestStatus.Failed)
                    {
                        localRequest.MarkAsFailed(externalData.ErrorMessage);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            var response = new InquireRequestResponse
            {
                RequestId = externalData.RequestId,
                ExternalRequestId = externalData.ExternalRequestId,
                Status = externalData.Status,
                SubmittedAt = externalData.SubmittedAt,
                CompletedAt = externalData.CompletedAt,
                ResponseData = externalData.ResponseData,
                ErrorMessage = externalData.ErrorMessage,
                AdditionalInfo = externalData.AdditionalInfo
            };

            _logger.LogInformation(
                "Successfully retrieved inquiry data for {LookupId}, Status: {Status}", 
                lookupId, 
                response.Status);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inquiry request");
            return Result.Failure<InquireRequestResponse>($"Internal error: {ex.Message}");
        }
    }
}
