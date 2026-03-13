using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party;

public interface ICustomerLookup
{
    Task<IReadOnlyList<CustomerLookupDto>> GetAllCustomerAsync();
}
