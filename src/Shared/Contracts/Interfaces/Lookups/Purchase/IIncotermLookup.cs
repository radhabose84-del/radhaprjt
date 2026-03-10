#nullable disable
using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase
{
    public interface IIncotermLookup
    {
        Task<List<IncotermLookupDto>> GetAllIncotermAsync();
    }
}
