using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption;

namespace MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption
{
    public interface IPowerConsumptionQueryRepository
    {
        Task<List<GetFeederSubFeederDto>> GetFeederSubFeedersById(int feederTypeId);
        Task<GetClosingReaderValueDto?> GetOpeningReaderValueById(int feederId);
        Task<(List<GetPowerConsumptionDto>, int)> GetAllPowerConsumptionAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<GetPowerConsumptionDto> GetPowerConsumptionById(int id);

    }
}