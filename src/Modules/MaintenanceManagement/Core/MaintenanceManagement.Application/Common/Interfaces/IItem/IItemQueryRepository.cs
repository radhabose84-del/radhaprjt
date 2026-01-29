using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.IItem
{
    public interface IItemQueryRepository
    {
        Task<List<MaintenanceManagement.Domain.Entities.ItemGroupCode>> GetGroupCodes(string OldUnitId);
        Task<List<MaintenanceManagement.Domain.Entities.ItemMaster>> GetItemMasters(string OldUnitId,string Grpcode,string? ItemCode,string? ItemName);
    }
}