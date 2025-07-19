using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class LearnIrCommandRequestValidator : Validator<LearnIrCommandRequest>
{
    public LearnIrCommandRequestValidator()
    {
        RuleFor(x => x.CommandName)
            .NotEmpty().WithMessage("Command name is required")
            .MaximumLength(100).WithMessage("Command name must not exceed 100 characters");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 60).WithMessage("Timeout must be between 1 and 60 seconds");
    }
}