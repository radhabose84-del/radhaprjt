using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Infrastructure.Data;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.GeneratorConsumption
{
    public class GeneratorConsumptionCommandRepository : IGeneratorConsumptionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public GeneratorConsumptionCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption generatorConsumption)
        {
            // Add the PowerConsumption to the DbContext
                await _applicationDbContext.GeneratorConsumption.AddAsync(generatorConsumption);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created PowerConsumption
                return generatorConsumption.Id;
        }
    }
}