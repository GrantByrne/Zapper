using FastEndpoints;
using FluentValidation;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators.UsbRemotes;

public class UpdateUsbRemoteRequestValidator : Validator<UpdateUsbRemoteRequest>
{
    public UpdateUsbRemoteRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Remote ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Remote name is required")
            .MaximumLength(100).WithMessage("Remote name must not exceed 100 characters");

        RuleFor(x => x.LongPressTimeoutMs)
            .InclusiveBetween(100, 5000).WithMessage("Long press timeout must be between 100 and 5000 milliseconds");
    }
}