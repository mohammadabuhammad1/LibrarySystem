using FluentValidation;
using LibrarySystem.Domain.Commands.Books;

namespace LibrarySystem.Application.Validators.Commands;

public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Book ID must be greater than 0");
    }
}