using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder
{
    public interface IFeederCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.Feeder feeder);
        Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.Power.Feeder feeder);
        Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.Power.Feeder feeder); 
    }
}