using Application.Features.ToDos.Handlers;
using FluentValidation;

namespace Application.Features.ToDos.Validators
{
    internal class UpdateToDoValidator : AbstractValidator<UpdateToDoCommand>
    {
        public UpdateToDoValidator()
        {
            RuleFor(t => t.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

            RuleFor(t => t.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(t => t.ExpiryDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("ExpiryDate must be in the future.");
        }
    }
}
