using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Warehouse;

namespace Contracts.Interfaces.External.IWarehouse
{
    public interface IRackGrpcClient
    {
        Task<PagedResult<RackDto>> GetAllAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default);
       Task<RackDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
    public sealed class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
