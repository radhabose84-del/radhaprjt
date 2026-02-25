using MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetClosingEnergyReaderValueById;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetGeneratorConsumption;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetUnitIdBasedOnMachineId;


namespace MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption
{
    public interface IGeneratorConsumptionQueryRepository
    {
        Task<GetClosingEnergyReaderValueDto> GetOpeningReaderValueById(int generatorId);
        Task<List<GetMachineIdBasedonUnitDto>> GetMachineIdBasedonUnit();
        Task<(List<GetGeneratorConsumptionDto>, int)> GetAllGeneratorConsumptionAsync(int PageNumber, int PageSize, string? SearchTerm);
        
    }
}