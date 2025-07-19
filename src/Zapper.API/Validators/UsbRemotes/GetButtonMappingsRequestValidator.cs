using FastEndpoints;
using FluentValidation;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators.UsbRemotes;

public class GetButtonMappingsRequestValidator : Validator<GetButtonMappingsRequest>
{
    public GetButtonMappingsRequestValidator()
    {
        RuleFor(x => x.RemoteId)
            .GreaterThan(0).WithMessage("Remote ID must be greater than 0");
    }
}