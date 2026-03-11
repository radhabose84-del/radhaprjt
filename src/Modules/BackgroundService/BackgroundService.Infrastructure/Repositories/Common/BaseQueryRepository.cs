

using Contracts.Interfaces;

namespace BackgroundService.Infrastructure.Repositories.Common
{
   public class BaseQueryRepository
    {
        protected readonly IIPAddressService _ipAddressService;
        protected int CompanyId => _ipAddressService.GetCompanyId() ?? 0;
        protected int UnitId => _ipAddressService.GetUnitId() ?? 0;
        protected string ipAddress => _ipAddressService.GetSystemIPAddress();
        protected int createdBy => _ipAddressService.GetUserId();
        protected string createdName => _ipAddressService.GetUserName();

        // ✅ Accept the interface here
        protected BaseQueryRepository(IIPAddressService ipAddressService)
        {
            _ipAddressService = ipAddressService;
        }
    }
}