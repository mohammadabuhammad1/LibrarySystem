using FluentValidation;
using LibrarySystem.Domain.Commands.Books;

namespace LibrarySystem.Application.Validators.Commands;

public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Book ID must be greater than 0");


        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty if provided.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .MinimumLength(1).WithMessage("Title must be at least 1 character")
            .When(x => !string.IsNullOrEmpty(x.Title)); 

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required if provided.")
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Author name must be at least 2 characters")
            .When(x => !string.IsNullOrEmpty(x.Author)); 

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1000, DateTime.UtcNow.Year + 1)
            .WithMessage($"Published year must be between 1000 and {DateTime.UtcNow.Year + 1}")
            .When(x => x.PublishedYear.HasValue); 

        RuleFor(x => x.TotalCopies)
            .GreaterThanOrEqualTo(0).WithMessage("Total copies cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Cannot have more than 1000 copies")
            .When(x => x.TotalCopies.HasValue);


        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description != null); 

        RuleFor(x => x.Genre)
            .MaximumLength(50).WithMessage("Genre cannot exceed 50 characters")
            .When(x => x.Genre != null); 
    }
}