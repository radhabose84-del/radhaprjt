using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users;

public interface IMenuLookup
{
    Task<List<MenuLookupDto>> GetAllMenuAsync();
}
