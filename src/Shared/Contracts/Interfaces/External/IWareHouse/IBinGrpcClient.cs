// Contracts/Interfaces/External/IWarehouse/IBinGrpcClient.cs
using Contracts.Dtos.Warehouse;

namespace Contracts.Interfaces.External.IWarehouse
{
    public interface IBinGrpcClient
    {
        Task<List<BinDto>> GetAllBinMasterAsync(
            int warehouseId,
            int? rackId = null,
            string? search = null,
            bool onlyActive = true,
            CancellationToken ct = default);
        Task<BinDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
