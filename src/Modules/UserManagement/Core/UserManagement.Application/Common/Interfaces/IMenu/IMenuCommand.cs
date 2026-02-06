using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IMenu
{
    public interface IMenuCommand
    {
        Task<int> CreateAsync(UserManagement.Domain.Entities.Menu menu);
        Task<bool> UpdateAsync(UserManagement.Domain.Entities.Menu menu);
        Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Menu menu);  
        Task<bool> BulkImportMenuAsync(List<UserManagement.Domain.Entities.Menu> menus);
    }
}