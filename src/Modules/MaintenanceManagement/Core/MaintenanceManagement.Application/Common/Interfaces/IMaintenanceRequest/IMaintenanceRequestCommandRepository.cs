using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest
{
    public interface IMaintenanceRequestCommandRepository  
    {
          Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest);   
       
            Task<bool> UpdateAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest);

            Task<bool> UpdateStatusAsync(int id);

    }
}