using FastEndpoints;
using FluentValidation;
using Zapper.Client;

namespace Zapper.API.Validators;

public class CreateActivityRequestValidator : Validator<CreateActivityRequest>
{
    public CreateActivityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Activity name is required")
            .MaximumLength(100).WithMessage("Activity name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Activity type is required")
            .Must(BeAValidActivityType).WithMessage("Invalid activity type");

        RuleFor(x => x.Steps)
            .NotNull().WithMessage("Steps collection cannot be null");

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.DeviceId)
                .GreaterThan(0).WithMessage("Device ID must be greater than 0");

            step.RuleFor(s => s.Command)
                .NotEmpty().WithMessage("Command is required");

            step.RuleFor(s => s.DelayMs)
                .GreaterThanOrEqualTo(0).WithMessage("Delay must be non-negative");

            step.RuleFor(s => s.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative");
        });
    }

    private bool BeAValidActivityType(string type)
    {
        var validTypes = new[] { "Composite", "Macro", "Scene" };
        return validTypes.Contains(type);
    }
}