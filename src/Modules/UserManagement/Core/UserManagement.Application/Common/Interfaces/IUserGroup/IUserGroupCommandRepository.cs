
namespace UserManagement.Application.Common.Interfaces.IUserGroup
{
    public interface IUserGroupCommandRepository
    {
        Task<UserManagement.Domain.Entities.UserGroup> CreateAsync(UserManagement.Domain.Entities.UserGroup country);        
        Task<int>  UpdateAsync(int countryId,UserManagement.Domain.Entities.UserGroup user);
        Task<int>  DeleteAsync(int countryId,UserManagement.Domain.Entities.UserGroup country);    
        Task<UserManagement.Domain.Entities.UserGroup> GetUserGroupByCodeAsync(string groupName,string groupCode);     
    }
}