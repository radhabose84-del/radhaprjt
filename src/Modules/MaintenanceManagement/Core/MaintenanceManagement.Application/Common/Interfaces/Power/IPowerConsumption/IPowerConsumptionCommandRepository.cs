namespace MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption
{
    public interface IPowerConsumptionCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.PowerConsumption powerConsumption);
    }
}