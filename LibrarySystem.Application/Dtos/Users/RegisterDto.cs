﻿using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Application.Dtos.Users;

public class RegisterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{6,}$",
        ErrorMessage = "Password must be at least 6 characters and include uppercase, lowercase, a digit, and a special character.")]
    public string Password { get; set; } = string.Empty;
}