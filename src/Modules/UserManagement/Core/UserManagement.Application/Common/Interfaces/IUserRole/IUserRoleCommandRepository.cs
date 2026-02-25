namespace UserManagement.Application.Common.Interfaces.IUserRole
{
    using UserRole = UserManagement.Domain.Entities.UserRole;
    public interface IUserRoleCommandRepository
    {
        
        Task<UserRole> CreateAsync(UserRole userrole);
        Task<int> DeleteAsync(int id, UserRole userrole);
        Task<int> UpdateAsync(int id, UserRole userrole);
        Task<bool> ExistsByCodeAsync(string userrole);
       
        Task<bool> ExistsByNameupdateAsync(string roleName, int id);
        
    }
}