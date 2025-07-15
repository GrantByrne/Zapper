using FastEndpoints;
using FluentValidation;
using Zapper.Contracts;

namespace Zapper.API.Validators;

public class UpdateActivityRequestValidator : Validator<UpdateActivityRequest>
{
    public UpdateActivityRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Activity ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Activity name is required")
            .MaximumLength(100).WithMessage("Activity name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Activity type is required")
            .Must(BeAValidActivityType).WithMessage("Invalid activity type");
    }

    private bool BeAValidActivityType(string type)
    {
        var validTypes = new[] { "Composite", "Macro", "Scene" };
        return validTypes.Contains(type);
    }
}