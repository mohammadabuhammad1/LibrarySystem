using FluentValidation;
using LibrarySystem.Application.Dtos.Libraries;
using System;


namespace LibrarySystem.Application.Validators.Libraries;
public class UpdateLibraryDtoValidator : AbstractValidator<UpdateLibraryDto>
{
    public UpdateLibraryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Library name is required")
            .MaximumLength(100).WithMessage("Library name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Library name must be at least 2 characters")
            .Matches(@"^[a-zA-Z0-9\s\-\.&',]+$").WithMessage("Library name contains invalid characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters")
            .MinimumLength(5).WithMessage("Location must be at least 5 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}