using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<Domain.Entities.Notification.MiscMaster> CreateAsync(Domain.Entities.Notification.MiscMaster miscMaster);  

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, Domain.Entities.Notification.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id,Domain.Entities.Notification.MiscMaster miscMaster);  
    }
}