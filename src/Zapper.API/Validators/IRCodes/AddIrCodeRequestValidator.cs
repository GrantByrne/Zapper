using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class AddIrCodeRequestValidator : Validator<AddIrCodeRequest>
{
    public AddIrCodeRequestValidator()
    {
        RuleFor(x => x.CodeSetId)
            .GreaterThan(0).WithMessage("Code Set ID must be greater than 0");

        RuleFor(x => x.Code)
            .NotNull().WithMessage("IR Code is required");

        When(x => x.Code != null, () =>
        {
            RuleFor(x => x.Code.Brand)
                .NotEmpty().WithMessage("Brand is required")
                .MaximumLength(100).WithMessage("Brand must not exceed 100 characters");

            RuleFor(x => x.Code.Model)
                .NotEmpty().WithMessage("Model is required")
                .MaximumLength(100).WithMessage("Model must not exceed 100 characters");

            RuleFor(x => x.Code.DeviceType)
                .IsInEnum().WithMessage("Invalid device type");

            RuleFor(x => x.Code.CommandName)
                .NotEmpty().WithMessage("Command name is required")
                .MaximumLength(100).WithMessage("Command name must not exceed 100 characters");

            RuleFor(x => x.Code.Protocol)
                .NotEmpty().WithMessage("Protocol is required")
                .MaximumLength(50).WithMessage("Protocol must not exceed 50 characters");

            RuleFor(x => x.Code.HexCode)
                .NotEmpty().WithMessage("Hex code is required");

            RuleFor(x => x.Code.Frequency)
                .GreaterThan(0).WithMessage("Frequency must be greater than 0");
        });
    }
}