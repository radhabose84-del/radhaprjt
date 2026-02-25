using Contracts.Dtos.Maintenance;

namespace Contracts.Interfaces.External.IMaintenance
{
    public interface ICostCenterGrpcClient
    {
        Task<CostCenterGrpcDto?> GetCostCenterByIdAsync(int id);
        Task<List<CostCenterGrpcDto>> GetAllCostCentersAsync();
    }
}
