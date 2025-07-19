using FastEndpoints;
using FluentValidation;
using Zapper.Client.System;

namespace Zapper.API.Validators.System;

public class TestGpioPinRequestValidator : Validator<TestGpioPinRequest>
{
    public TestGpioPinRequestValidator()
    {
        RuleFor(x => x.Pin)
            .InclusiveBetween(1, 40).WithMessage("Pin number must be between 1 and 40");
    }
}