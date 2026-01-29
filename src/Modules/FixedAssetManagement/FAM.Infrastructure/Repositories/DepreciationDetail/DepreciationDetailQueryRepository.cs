
using System.Data;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Common;
using Dapper;
using FAM.Infrastructure.Data;
using FAM.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.DepreciationDetail
{
    public class DepreciationDetailQueryRepository : BaseQueryRepository, IDepreciationDetailQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ApplicationDbContext _applicationDbContext;
        
        public DepreciationDetailQueryRepository(IDbConnection dbConnection,ApplicationDbContext applicationDbContext,IIPAddressService ipAddressService)
        : base(ipAddressService) 
        {
            _dbConnection = dbConnection;
            _applicationDbContext = applicationDbContext;        
        }
    
        public async Task<(List<DepreciationDto>, int,bool, string)> CalculateDepreciationAsync(int companyId, int unitId, int finYearId, DateTimeOffset? startDate, DateTimeOffset? endDate,int depreciationType, int PageNumber, int PageSize, string? SearchTerm,int depreciationPeriod)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@UnitId", unitId);
            parameters.Add("@Finyear", finYearId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);
            parameters.Add("@Type", depreciationType);            
            parameters.Add("@Save", 0);
            parameters.Add("@Period", depreciationPeriod);            
            parameters.Add("@PageNumber", PageNumber );
            parameters.Add("@PageSize", PageSize );
            parameters.Add("@SearchTerm", SearchTerm);
        
         using var multiResult = await _dbConnection.QueryMultipleAsync(
        "dbo.FAM_DepreciationCalculation", parameters, commandType: CommandType.StoredProcedure);

        // Read first result set as dynamic to check for error
        var firstResult = await multiResult.ReadAsync();
        var firstRow = firstResult.FirstOrDefault();

        if (firstRow is IDictionary<string, object> dict &&
            dict.TryGetValue("StatusCode", out var statusCodeObj) &&
            Convert.ToInt32(statusCodeObj) != 0)
        {
            // Read the dummy second result
            await multiResult.ReadAsync(); // SELECT 0
            string message = dict.TryGetValue("Status", out var statusMessageObj)
                ? statusMessageObj?.ToString()
                : "Unknown error";
            return (new List<DepreciationDto>(), 0, false, message ?? "Unknown error");
        }

        // ✅ Re-read result set as DepreciationDto
        var depreciationList = (await _dbConnection.QueryAsync<DepreciationDto>(
            "dbo.FAM_DepreciationCalculation", parameters, commandType: CommandType.StoredProcedure)).ToList();

        // ✅ Get total count separately if needed
        int totalCount = depreciationList.Count;

        return (depreciationList, totalCount, true, "");
    
        }

        public async Task<(string message, int statusCode)>  CreateAsync( int finYearId,  int depreciationType,int depreciationPeriod)
        {
            int userId = _ipAddressService.GetUserId();
            string username = _ipAddressService.GetUserName();
            string ipAddress = _ipAddressService.GetSystemIPAddress();

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", CompanyId);
            parameters.Add("@UnitId", UnitId);
            parameters.Add("@Finyear", finYearId);
            parameters.Add("@StartDate", null);
            parameters.Add("@EndDate", null);
            parameters.Add("@Type", depreciationType);
            parameters.Add("@Save", 1);
            parameters.Add("@Period", depreciationPeriod);            
            parameters.Add("@PageNumber", 0);
            parameters.Add("@PageSize", 0);
            parameters.Add("@SearchTerm", "");
            parameters.Add("@AssetId", 0);
            parameters.Add("@CreatedBy",userId);
            parameters.Add("@CreatedByName",  username);
            parameters.Add("@CreatedIP", ipAddress);

           // Execute the stored procedure (ignoring return message)
     /*      var result =  await _dbConnection.ExecuteAsync(
                "dbo.FAM_DepreciationCalculation",
                parameters,
                commandType: CommandType.StoredProcedure
            );    */
           var result = await _dbConnection.QueryAsync(
                "dbo.FAM_DepreciationCalculation",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Assuming your stored procedure returns 'Status' and 'StatusCode'
            var firstResult = result.FirstOrDefault();
            if (firstResult != null)
            {
                return (firstResult.Status, firstResult.StatusCode);
            }

            return ("Unknown error", -1);
        } 

        public async Task<bool> ExistDataAsync( int finYearId, int depreciationType,int depreciationPeriod)
        {
            return await _applicationDbContext.DepreciationDetails
                .Where(d => d.CompanyId == CompanyId &&
                            d.Finyear == finYearId &&
                            d.DepreciationPeriod == depreciationPeriod &&
                            d.DepreciationType == depreciationType &&                                                    
                            (UnitId == 0 || d.UnitId == UnitId)) 
                .AnyAsync();
        }
        public async Task<bool> ExistDataLockedAsync( int finYearId, int depreciationType,int depreciationPeriod)
        {
            return await _applicationDbContext.DepreciationDetails
                .Where(d => d.CompanyId == CompanyId &&
                            d.Finyear == finYearId &&
                            d.DepreciationPeriod == depreciationPeriod &&
                            d.DepreciationType == depreciationType &&  d.IsLocked ==1 &&                                             
                            (UnitId == 0 || d.UnitId == UnitId)) 
                .AnyAsync();
        }

        public async Task<List<DepreciationAbstractDto>> GetDepreciationAbstractAsync (int companyId, int unitId, int finYearId, DateTimeOffset? startDate,DateTimeOffset? endDate,int depreciationPeriod, int depreciationType)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@UnitId", unitId);
            parameters.Add("@Finyear", finYearId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);
            parameters.Add("@Type", depreciationType);     
            parameters.Add("@Save", 0); 
            parameters.Add("@Period", depreciationPeriod);
            parameters.Add("@PageNumber", 0);     
            parameters.Add("@PageSize", 0);     
            parameters.Add("@SearchTerm", "");     
            parameters.Add("@AssetId", 0);     
            parameters.Add("@CreatedBy", 0);     
            parameters.Add("@CreatedByName", "");     
            parameters.Add("@CreatedIP", "");
            parameters.Add("@Consolidate", 1);                                

            var result = await _dbConnection.QueryAsync<DepreciationAbstractDto>("dbo.FAM_DepreciationCalculation", parameters, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetDepreciationMethodAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.DeprecationPeriod.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
    }
}