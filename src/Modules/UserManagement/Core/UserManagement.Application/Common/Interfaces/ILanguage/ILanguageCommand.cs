namespace UserManagement.Application.Common.Interfaces.ILanguage
{
    public interface ILanguageCommand
    {
         Task<UserManagement.Domain.Entities.Language> CreateAsync(UserManagement.Domain.Entities.Language language);     
         Task<bool> UpdateAsync(UserManagement.Domain.Entities.Language language);
        Task<bool> DeleteAsync(int id,UserManagement.Domain.Entities.Language language);   
    }
}