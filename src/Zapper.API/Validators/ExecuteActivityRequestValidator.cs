using FastEndpoints;
using FluentValidation;
using Zapper.Client.Activities;

namespace Zapper.API.Validators;

public class ExecuteActivityRequestValidator : Validator<ExecuteActivityRequest>
{
    public ExecuteActivityRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Activity ID must be greater than 0");
    }
}