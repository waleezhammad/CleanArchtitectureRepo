using FluentValidation;

namespace IntegrationService.Application.Requests.Commands.AddRequest;

/// <summary>
/// Validator for AddRequestCommand
/// </summary>
public class AddRequestCommandValidator : AbstractValidator<AddRequestCommand>
{
    public AddRequestCommandValidator()
    {
        RuleFor(x => x.RequestType)
            .NotEmpty().WithMessage("Request type is required")
            .MaximumLength(50).WithMessage("Request type must not exceed 50 characters");

        RuleFor(x => x.RequestData)
            .NotEmpty().WithMessage("Request data is required")
            .MaximumLength(10000).WithMessage("Request data must not exceed 10000 characters");

        RuleFor(x => x.Metadata)
            .NotNull().WithMessage("Metadata cannot be null");
    }
}
