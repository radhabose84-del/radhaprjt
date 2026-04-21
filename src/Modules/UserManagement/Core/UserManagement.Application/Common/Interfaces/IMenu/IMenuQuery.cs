namespace UserManagement.Application.Common.Interfaces.IMenu
{
    public interface IMenuQuery
    {
        Task<List<UserManagement.Domain.Entities.Menu>> GetParentMenus(List<int> moduleId);
        Task<List<UserManagement.Domain.Entities.Menu>> GetChildMenus(List<int> ParentId);
        Task<bool> FKColumnExistValidation(int Id);
        Task<(IEnumerable<dynamic>, int)> GetAllMenuAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<UserManagement.Domain.Entities.Menu> GetMenuByNameAsync(string MenuName);
        Task<List<UserManagement.Domain.Entities.Menu>> GetParentMenuAutoComplete(string searchPattern, int? moduleId = null);
        Task<List<UserManagement.Domain.Entities.Menu>> GetMenusByIds(IEnumerable<int> ids, CancellationToken ct = default);
        
    }
}