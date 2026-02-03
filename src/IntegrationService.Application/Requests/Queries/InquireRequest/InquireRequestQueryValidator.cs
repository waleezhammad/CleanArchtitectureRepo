using FluentValidation;

namespace IntegrationService.Application.Requests.Queries.InquireRequest;

/// <summary>
/// Validator for InquireRequestQuery
/// </summary>
public class InquireRequestQueryValidator : AbstractValidator<InquireRequestQuery>
{
    public InquireRequestQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.RequestId) || !string.IsNullOrEmpty(x.ExternalRequestId))
            .WithMessage("Either RequestId or ExternalRequestId must be provided");

        RuleFor(x => x.RequestId)
            .MaximumLength(100).WithMessage("RequestId must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.RequestId));

        RuleFor(x => x.ExternalRequestId)
            .MaximumLength(100).WithMessage("ExternalRequestId must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ExternalRequestId));
    }
}
