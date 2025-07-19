using FastEndpoints;
using FluentValidation;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators.UsbRemotes;

public class GetUsbRemoteRequestValidator : Validator<GetUsbRemoteRequest>
{
    public GetUsbRemoteRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Remote ID must be greater than 0");
    }
}