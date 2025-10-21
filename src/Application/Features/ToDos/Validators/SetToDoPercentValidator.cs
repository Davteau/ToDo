using Application.Features.ToDos.Handlers;
using FluentValidation;

namespace Application.Features.ToDos.Validators;

internal sealed class SetToDoPercentValidator : AbstractValidator<SetToDoPercentCommand>
{
    public SetToDoPercentValidator()
    {
        RuleFor(t => t.PercentComplete)
            .InclusiveBetween(0, 100).WithMessage("PercentComplete must be between 0 and 100.");
    }
}
