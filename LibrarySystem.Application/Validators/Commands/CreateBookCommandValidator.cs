using FluentValidation;
using LibrarySystem.Domain.Commands.Books;


namespace LibrarySystem.Application.Validators.Commands;
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .MinimumLength(1).WithMessage("Title must be at least 1 character");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required")
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Author name must be at least 2 characters");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required")
            .Matches(@"^(?:\d{9}[\dX]|\d{13})$").WithMessage("ISBN must be valid 10 or 13 digits");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1000, DateTime.UtcNow.Year + 1)
            .WithMessage($"Published year must be between 1000 and {DateTime.UtcNow.Year + 1}");

        RuleFor(x => x.TotalCopies)
            .GreaterThan(0).WithMessage("Total copies must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Cannot have more than 1000 copies");

        //RuleFor(x => x.LibraryId)
        //    .GreaterThan(0).When(x => x.LibraryId.HasValue)
        //    .WithMessage("Library ID must be greater than 0")

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Genre)
            .MaximumLength(50).WithMessage("Genre cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Genre));
    }
}
