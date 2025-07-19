using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class GetIrCodeRequestValidator : Validator<GetIrCodeRequest>
{
    public GetIrCodeRequestValidator()
    {
        RuleFor(x => x.CodeSetId)
            .GreaterThan(0).WithMessage("Code Set ID must be greater than 0");

        RuleFor(x => x.CommandName)
            .NotEmpty().WithMessage("Command name is required")
            .MaximumLength(100).WithMessage("Command name must not exceed 100 characters");
    }
}