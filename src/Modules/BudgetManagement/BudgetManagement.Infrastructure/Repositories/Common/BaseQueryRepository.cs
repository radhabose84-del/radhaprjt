

using BudgetManagement.Application.Common.Interfaces;

namespace BudgetManagement.Infrastructure.Repositories.Common
{
   public class BaseQueryRepository
    {
        protected readonly IIPAddressService _ipAddressService;
        protected int CompanyId => _ipAddressService.GetCompanyId();
        protected int UnitId => _ipAddressService.GetUnitId();

        // ✅ Accept the interface here
        protected BaseQueryRepository(IIPAddressService ipAddressService)
        {
            _ipAddressService = ipAddressService;
        }
    }
}