﻿namespace LibrarySystem.Application.Dtos.Libraries;

public class CreateLibraryDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrganizationUnitId { get; set; } 
}
