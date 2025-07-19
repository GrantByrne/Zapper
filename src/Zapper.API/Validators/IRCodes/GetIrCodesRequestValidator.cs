using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class GetIrCodesRequestValidator : Validator<GetIrCodesRequest>
{
    public GetIrCodesRequestValidator()
    {
        RuleFor(x => x.CodeSetId)
            .GreaterThan(0).WithMessage("Code Set ID must be greater than 0");
    }
}