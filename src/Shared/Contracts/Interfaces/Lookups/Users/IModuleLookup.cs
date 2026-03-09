using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users;

public interface IModuleLookup
{
    Task<List<ModuleLookupDto>> GetAllModuleAsync();
}
