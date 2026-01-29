using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IHSNGrpcClient
    {
        Task<(List<HSNMasterDto>, int)> GetAllAsync(int PageNumber, int PageSize, string SearchTerm);
        Task<HSNMasterDto> GetByIdAsync(int id);
    }
}
