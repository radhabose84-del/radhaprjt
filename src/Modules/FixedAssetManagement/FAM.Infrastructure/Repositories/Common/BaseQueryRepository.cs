
using Contracts.Interfaces;

namespace FAM.Infrastructure.Repositories.Common
{
    public class BaseQueryRepository
    {
        protected readonly IIPAddressService _ipAddressService;

        protected int CompanyId => _ipAddressService.GetCompanyId() ?? 0;
        protected int UnitId => _ipAddressService.GetUnitId() ?? 0;
        protected string OldUnitId => _ipAddressService.GetOldUnitId();        
        protected string UserName => _ipAddressService.GetUserName(); 
        protected int UserId => _ipAddressService.GetUserId(); 
        protected string UserIPAddress => _ipAddressService.GetUserIPAddress(); 

        // ✅ Accept the interface here
        protected BaseQueryRepository(IIPAddressService ipAddressService)
        {
            _ipAddressService = ipAddressService;
        }
    }
}