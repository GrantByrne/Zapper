using FastEndpoints;
using FluentValidation;
using Zapper.Client;

namespace Zapper.API.Validators.Activities;

public class GetActivityRequestValidator : Validator<GetActivityRequest>
{
    public GetActivityRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Activity ID must be greater than 0");
    }
}