
namespace Core.Application.Common.Interfaces.IUserGroup
{
    public interface IUserGroupCommandRepository
    {
        Task<Core.Domain.Entities.UserGroup> CreateAsync(Core.Domain.Entities.UserGroup country);        
        Task<int>  UpdateAsync(int countryId,Core.Domain.Entities.UserGroup user);
        Task<int>  DeleteAsync(int countryId,Core.Domain.Entities.UserGroup country);    
        Task<Core.Domain.Entities.UserGroup> GetUserGroupByCodeAsync(string groupName,string groupCode);     
    }
}