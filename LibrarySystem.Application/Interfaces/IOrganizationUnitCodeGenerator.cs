namespace LibrarySystem.Domain.Interfaces;

public interface IOrganizationUnitCodeGenerator
{
    Task<string> GenerateNextCodeAsync(int? parentId = null);
    Task<string> GetFullCodePathAsync(int ouId);
    Task<bool> CodeExistsAsync(string code);
    Task<IEnumerable<string>> GetDescendantCodesAsync(string parentCode);


}