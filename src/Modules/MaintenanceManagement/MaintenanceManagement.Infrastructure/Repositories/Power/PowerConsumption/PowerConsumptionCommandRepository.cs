using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Infrastructure.Data;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.PowerConsumption
{
    public class PowerConsumptionCommandRepository : IPowerConsumptionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
         public PowerConsumptionCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.PowerConsumption powerConsumption)
        {
             // Add the PowerConsumption to the DbContext
                await _applicationDbContext.PowerConsumption.AddAsync(powerConsumption);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created PowerConsumption
                return powerConsumption.Id;
        }

    }
}