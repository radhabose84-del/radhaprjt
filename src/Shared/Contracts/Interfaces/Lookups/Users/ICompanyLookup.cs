using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICompanyLookup
    {
      Task<List<CompanyLookupDto>> GetAllCompanyAsync();
    }
}