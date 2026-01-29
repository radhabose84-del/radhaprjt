
using FAM.Application.Common.Interfaces;
using FAM.Infrastructure.Services;

namespace FAM.Infrastructure.Repositories.Common
{
   public class BaseQueryRepository
    {
        protected readonly IIPAddressService _ipAddressService;

        protected int CompanyId => _ipAddressService.GetCompanyId();
        protected int UnitId => _ipAddressService.GetUnitId();
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