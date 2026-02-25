namespace MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption
{
    public interface IGeneratorConsumptionCommandRepository
    {
         Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption generatorConsumption);
    }
}