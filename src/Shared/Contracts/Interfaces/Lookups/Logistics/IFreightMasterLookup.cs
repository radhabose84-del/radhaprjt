using Contracts.Dtos.Lookups.Logistics;

namespace Contracts.Interfaces.Lookups.Logistics;

public interface IFreightMasterLookup
{
    Task<List<FreightMasterLookupDto>> GetAllFreightMasterAsync();
    Task<List<FreightMasterLookupDto>> GetByModuleIdAsync(int moduleId);
    Task<FreightMasterLookupDto?> GetByIdAsync(int id);
}
