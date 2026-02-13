using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ILocationLookup
    {
        Task<LocationLookupDto?> GetLocationAsync(string city, string state, string country, CancellationToken ct = default);
    }
}
