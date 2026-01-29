using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using FAM.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.DepreciationDetail
{
    public class DepreciationDetailCommandRepository : BaseQueryRepository,IDepreciationDetailCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DepreciationDetailCommandRepository(ApplicationDbContext applicationDbContext,IIPAddressService ipAddressService)
        : base(ipAddressService) 
        {
            _applicationDbContext = applicationDbContext;
        }

        
        public async Task<int> DeleteAsync( int finYearId, int depreciationType,int depreciationPeriod)
        {
            var depreciationDetails = await _applicationDbContext.DepreciationDetails
                .Where(d => d.CompanyId == CompanyId &&
                                           (UnitId == 0 || d.UnitId == UnitId) &&
                                          d.Finyear == finYearId &&
                                          d.DepreciationPeriod == depreciationPeriod && d.DepreciationType==depreciationType ).ToListAsync();        
            if (depreciationDetails != null)
            {
                _applicationDbContext.DepreciationDetails.RemoveRange(depreciationDetails); // Physically delete
                return await _applicationDbContext.SaveChangesAsync();                
            }   
            return 0;
        }

        public async Task<int> UpdateAsync(int finYearId, int depreciationType, int depreciationPeriod)
        {
            var depreciationDetails = await _applicationDbContext.DepreciationDetails
                .Where(d => d.CompanyId == CompanyId &&
                                           (UnitId == 0 || d.UnitId == UnitId) &&
                                          d.Finyear == finYearId &&
                                          d.DepreciationPeriod == depreciationPeriod && d.DepreciationType==depreciationType ).ToListAsync();        
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
