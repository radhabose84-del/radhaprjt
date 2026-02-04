namespace UserManagement.Application.Common.Interfaces.IUserGroup
{
    using UserGroup = UserManagement.Domain.Entities.UserGroup;
    public interface IUserGroupQueryRepository
    {        
        Task<List<UserGroup>> GetUserGroups (string searchPattern);
        Task<UserGroup> GetByIdAsync(int countryId);
        Task<(List<UserGroup>,int)> GetAllUserGroupAsync(int PageNumber, int PageSize, string? SearchTerm);  
        Task<bool> SoftDeleteValidation(int Id);  
    }
}