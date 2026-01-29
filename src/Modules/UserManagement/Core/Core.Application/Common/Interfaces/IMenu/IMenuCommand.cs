using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IMenu
{
    public interface IMenuCommand
    {
        Task<int> CreateAsync(Core.Domain.Entities.Menu menu);
        Task<bool> UpdateAsync(Core.Domain.Entities.Menu menu);
        Task<bool> DeleteAsync(int id, Core.Domain.Entities.Menu menu);  
        Task<bool> BulkImportMenuAsync(List<Core.Domain.Entities.Menu> menus);
    }
}