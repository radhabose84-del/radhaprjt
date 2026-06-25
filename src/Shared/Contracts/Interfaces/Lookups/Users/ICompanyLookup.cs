using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICompanyLookup
    {
      Task<List<CompanyLookupDto>> GetAllCompanyAsync();

      // US-GL02-10 (AC5) — companies the given user is actively assigned to (AppSecurity.UserCompany),
      // for the mandatory, profile-scoped company selector on the account screens.
      Task<List<CompanyLookupDto>> GetUserCompaniesAsync(int userId);
    }
}