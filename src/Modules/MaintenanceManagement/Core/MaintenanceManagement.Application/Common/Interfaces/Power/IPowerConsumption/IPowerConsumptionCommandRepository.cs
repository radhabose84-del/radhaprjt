using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption
{
    public interface IPowerConsumptionCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.PowerConsumption powerConsumption);
    }
}