using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement // keep existing namespace if used elsewhere
{
    public interface IUOMGrpcClient
    {
        Task<List<UOMMasterDto>> GetUOMAsync();
        Task<UOMMasterDto?> GetByIdAsync(int id , CancellationToken cancellationToken);
        Task<UOMMasterDto?> GetByNameAsync(string name, int? excludeId = null);
    }
}
