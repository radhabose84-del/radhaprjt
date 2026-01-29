using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Warehouse;

namespace Contracts.Interfaces.External.IWarehouse
{
    public interface IWarehouseGrpcClient
    {
        Task<PagedResult<WarehouseDto>> GetAllAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default);
        Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }   
}
