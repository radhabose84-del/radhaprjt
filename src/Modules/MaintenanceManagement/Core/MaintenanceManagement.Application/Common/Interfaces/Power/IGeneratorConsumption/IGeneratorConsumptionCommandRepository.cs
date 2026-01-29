using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption
{
    public interface IGeneratorConsumptionCommandRepository
    {
         Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption generatorConsumption);
    }
}