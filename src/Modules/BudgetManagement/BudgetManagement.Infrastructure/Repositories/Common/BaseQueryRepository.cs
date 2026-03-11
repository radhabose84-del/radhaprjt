

using Contracts.Interfaces;

namespace BudgetManagement.Infrastructure.Repositories.Common
{
   public class BaseQueryRepository
    {
        protected readonly IIPAddressService _ipAddressService;
        protected int CompanyId => _ipAddressService.GetCompanyId() ?? 0;
        protected int UnitId => _ipAddressService.GetUnitId() ?? 0;

        // ✅ Accept the interface here
        protected BaseQueryRepository(IIPAddressService ipAddressService)
        {
            _ipAddressService = ipAddressService;
        }
    }
}