namespace UserManagement.Application.Common.Interfaces.IAdminSecuritySettings
{
    public interface IAdminSecuritySettingsQueryRepository 
    {
        // Task<List<UserManagement.Domain.Entities.AdminSecuritySettings>> GetAllAdminSecuritySettingsAsync();

        Task<(List<UserManagement.Domain.Entities.AdminSecuritySettings>,int)> GetAllAdminSecuritySettingsAsync(int PageNumber, int PageSize, string? SearchTerm);
         Task<UserManagement.Domain.Entities.AdminSecuritySettings> GetAdminSecuritySettingsByIdAsync(int id);
         
         
    }
}