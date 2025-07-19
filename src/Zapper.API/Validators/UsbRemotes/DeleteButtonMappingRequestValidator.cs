using FastEndpoints;
using FluentValidation;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators.UsbRemotes;

public class DeleteButtonMappingRequestValidator : Validator<DeleteButtonMappingRequest>
{
    public DeleteButtonMappingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Button mapping ID must be greater than 0");
    }
}