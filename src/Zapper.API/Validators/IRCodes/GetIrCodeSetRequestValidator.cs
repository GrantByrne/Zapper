using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class GetIrCodeSetRequestValidator : Validator<GetIrCodeSetRequest>
{
    public GetIrCodeSetRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("IR Code Set ID must be greater than 0");
    }
}