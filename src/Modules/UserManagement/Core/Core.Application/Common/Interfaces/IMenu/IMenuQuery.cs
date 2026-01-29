using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IMenu
{
    public interface IMenuQuery
    {
        Task<List<Domain.Entities.Menu>> GetParentMenus(List<int> moduleId);
        Task<List<Domain.Entities.Menu>> GetChildMenus(List<int> ParentId);
        Task<bool> FKColumnExistValidation(int Id);
        Task<(IEnumerable<dynamic>, int)> GetAllMenuAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Core.Domain.Entities.Menu> GetMenuByNameAsync(string MenuName);
        Task<List<Core.Domain.Entities.Menu>> GetParentMenuAutoComplete(string searchPattern);
        Task<List<Core.Domain.Entities.Menu>> GetMenusByIds(IEnumerable<int> ids, CancellationToken ct = default);
        
    }
}