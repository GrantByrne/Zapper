using FastEndpoints;
using FluentValidation;
using Zapper.Contracts.Devices;

namespace Zapper.API.Validators;

public class SendCommandRequestValidator : Validator<SendCommandApiRequest>
{
    public SendCommandRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");

        RuleFor(x => x.Command)
            .NotEmpty().WithMessage("Command is required")
            .MaximumLength(100).WithMessage("Command must not exceed 100 characters");
    }
}