using FluentValidation;
using LibrarySystem.Domain.Commands.Books;

namespace LibrarySystem.Application.Validators.Commands;

public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    public ReturnBookCommandValidator()
    {
        RuleFor(x => x.BookId)
            .GreaterThan(0).WithMessage("Book ID must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(450).WithMessage("User ID cannot exceed 450 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Condition)
            .IsInEnum().WithMessage("Invalid book condition");

        RuleFor(x => x.FineAmount)
            .GreaterThanOrEqualTo(0).When(x => x.FineAmount.HasValue)
            .WithMessage("Fine amount cannot be negative");
    }
}