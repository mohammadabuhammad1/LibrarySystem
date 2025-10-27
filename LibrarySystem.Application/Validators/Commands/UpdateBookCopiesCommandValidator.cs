using FluentValidation;
using LibrarySystem.Domain.Commands.Books;

namespace LibrarySystem.Application.Validators.Commands;

public class UpdateBookCopiesCommandValidator : AbstractValidator<UpdateBookCopiesCommand>
{
    public UpdateBookCopiesCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Book ID must be greater than 0");

        RuleFor(x => x.TotalCopies)
            .GreaterThanOrEqualTo(0).WithMessage("Total copies cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Cannot have more than 1000 copies");

        RuleFor(x => x.Reason)
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}