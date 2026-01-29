
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;

namespace FAM.Application.Common.Interfaces.IDepreciationDetail
{
    public interface IDepreciationDetailQueryRepository
    {         
        Task<(string message, int statusCode)>  CreateAsync( int finYearId, int depreciationType,int depreciationPeriod);         
        Task<(List<DepreciationDto>,int,bool,string? )> CalculateDepreciationAsync(int companyId,int unitId, int finYearId, DateTimeOffset? startDate,DateTimeOffset? endDate,int DepreciationType, int PageNumber, int PageSize, string? SearchTerm,int depreciationPeriod);                        
        Task<bool> ExistDataAsync(int finYearId, int depreciationType,int depreciationPeriod);
        Task<bool> ExistDataLockedAsync(int finYearId, int depreciationType,int depreciationPeriod);
        Task<List<DepreciationAbstractDto>> GetDepreciationAbstractAsync ( int companyId,int unitId, int finYearId,DateTimeOffset? startDate,DateTimeOffset? endDate,int depreciationPeriod, int depreciationType);        
        Task<List<FAM.Domain.Entities.MiscMaster>> GetDepreciationMethodAsync();   
    }
}