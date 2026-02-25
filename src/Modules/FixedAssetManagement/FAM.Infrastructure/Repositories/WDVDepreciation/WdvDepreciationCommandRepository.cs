
using System.Data;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Infrastructure.Data;
using FAM.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.WDVDepreciation
{
    public class WdvDepreciationCommandRepository : BaseQueryRepository, IWdvDepreciationCommandRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ApplicationDbContext _applicationDbContext;

        public WdvDepreciationCommandRepository(IDbConnection dbConnection, ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        : base(ipAddressService)
        {
            _dbConnection = dbConnection;
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> DeleteAsync(int finYearId)
        {
            var depreciationDetails = await _applicationDbContext.WDVDepreciationDetail
                .Where(d => d.CompanyId == CompanyId &&                                           
                                          d.FinYear == finYearId && d.IsLocked==0).ToListAsync();        
            if (depreciationDetails != null)
            {
                _applicationDbContext.WDVDepreciationDetail.RemoveRange(depreciationDetails); // Physically delete
                return await _applicationDbContext.SaveChangesAsync();                
            }   
            return 0;
       }

        public async  Task<int> LockWDVDepreciationAsync(int finYearId)
        {
            var depreciationDetails = await _applicationDbContext.WDVDepreciationDetail
                .Where(d => d.CompanyId == CompanyId &&                                           
                                          d.FinYear == finYearId).ToListAsync();        
            if (depreciationDetails != null)
            {                 
                 foreach (var detail in depreciationDetails)
                {
                    detail.IsLocked =1;
                }                              
                return await _applicationDbContext.SaveChangesAsync();                
            }   
            return 0;
        }
    }
}
