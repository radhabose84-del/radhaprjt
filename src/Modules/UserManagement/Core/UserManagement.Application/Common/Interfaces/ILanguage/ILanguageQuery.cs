namespace UserManagement.Application.Common.Interfaces.ILanguage
{
    public interface ILanguageQuery
    {
        Task<(List<UserManagement.Domain.Entities.Language>,int)> GetAllLanguageAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<UserManagement.Domain.Entities.Language> GetByIdAsync(int id);
        Task<List<UserManagement.Domain.Entities.Language>> GetLanguage(string searchPattern);
        Task<UserManagement.Domain.Entities.Language?> GetByLanguagenameAsync(string name,int? id = null);
    }
}