using FastEndpoints;
using FluentValidation;
using Zapper.API.Endpoints.Devices;

namespace Zapper.API.Validators;

public class SendCommandRequestValidator : Validator<SendCommandRequest>
{
    public SendCommandRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");

        RuleFor(x => x.CommandName)
            .NotEmpty().WithMessage("Command name is required")
            .MaximumLength(100).WithMessage("Command name must not exceed 100 characters");
    }
}