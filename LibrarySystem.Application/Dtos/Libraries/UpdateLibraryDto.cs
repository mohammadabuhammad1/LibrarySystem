﻿namespace LibrarySystem.Application.Dtos.Libraries;

public class UpdateLibraryDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
}
