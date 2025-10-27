using FluentValidation;
using LibrarySystem.Domain.Commands.Books;

namespace LibrarySystem.Application.Validators.Commands;

public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(x => x.BookId)
            .GreaterThan(0).WithMessage("Book ID must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(450).WithMessage("User ID cannot exceed 450 characters");

        RuleFor(x => x.BorrowDurationDays)
            .InclusiveBetween(1, 90).WithMessage("Borrow duration must be between 1 and 90 days");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}