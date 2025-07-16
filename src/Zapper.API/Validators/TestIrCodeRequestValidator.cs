using FastEndpoints;
using FluentValidation;
using Zapper.Contracts.IRCodes;

namespace Zapper.API.Validators;

public class TestIrCodeRequestValidator : Validator<TestIrCodeRequest>
{
    public TestIrCodeRequestValidator()
    {
        RuleFor(x => x.CodeSetId)
            .GreaterThan(0).WithMessage("Code set ID must be greater than 0");

        RuleFor(x => x.CommandName)
            .NotEmpty().WithMessage("Command name is required")
            .MaximumLength(100).WithMessage("Command name must not exceed 100 characters");
    }
}