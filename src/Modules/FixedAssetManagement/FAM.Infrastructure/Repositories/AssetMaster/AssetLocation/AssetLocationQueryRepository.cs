using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Entities.AssetMaster;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetLocation
{
    public class AssetLocationQueryRepository :IAssetLocationQueryRepository
    {
         private readonly IDbConnection _dbConnection;

         public AssetLocationQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<FAM.Domain.Entities.AssetMaster.AssetLocation>, int)> GetAllAssetLocationAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
        DECLARE @TotalCount INT;
        SELECT @TotalCount = COUNT(*) 
        FROM FixedAsset.AssetLocation 
        WHERE 1 = 1
        {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CAST(AssetId AS NVARCHAR) LIKE @Search OR CAST(UnitId AS NVARCHAR) LIKE @Search)")}};

        SELECT Id, AssetId, UnitId, DepartmentId, LocationId, SubLocationId , CustodianId , UserID
        FROM FixedAsset.AssetLocation 
        WHERE 1 = 1
        {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CAST(AssetId AS NVARCHAR) LIKE @Search OR CAST(UnitId AS NVARCHAR) LIKE @Search)")}}
        ORDER BY Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT @TotalCount AS TotalCount;
        """;

        var parameters = new
        {
            Search = $"%{SearchTerm}%",
            Offset = (PageNumber - 1) * PageSize,
            PageSize
        };

        var assetLocations = await _dbConnection.QueryMultipleAsync(query, parameters);
        var assetLocationList = (await assetLocations.ReadAsync<FAM.Domain.Entities.AssetMaster.AssetLocation>()).ToList();
        int totalCount = await assetLocations.ReadFirstAsync<int>();

        return (assetLocationList, totalCount);
        }
        
        public async Task<FAM.Domain.Entities.AssetMaster.AssetLocation> GetByIdAsync(int id)
        {
           
             const string query = @"
                SELECT Id, AssetId, UnitId, DepartmentId, LocationId, SubLocationId, CustodianId , UserID
                FROM FixedAsset.AssetLocation 
                WHERE AssetId = @id   ";

            return  await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetMaster.AssetLocation>(query, new { id });

        }
         public async Task<FAM.Domain.Entities.AssetMaster.AssetLocation?> GetByAssetLocationCodeAsync(int? id = null)
        {
              var query = """
                 SELECT Id, AssetId, UnitId, DepartmentId, LocationId, SubLocationId, CustodianId , UserID FROM FixedAsset.AssetLocation
                 WHERE assetId = @id
                 """;

             var parameters = new DynamicParameters();

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetMaster.AssetLocation>(query, parameters);
        } 

         public async Task<(List<FAM.Domain.Entities.AssetMaster.Employee>, int)> GetAllCustodianAsync(string OldUnitId, string? SearchEmployee)
        {        

            var procedureName = "dbo.GetEmployeeByDivision";  // Name of the stored procedure

            var parameters = new
            {
                DivCode = OldUnitId,   // Mapping OldUnitId to DivCode
                EmpNo = SearchEmployee // Search criteria for employee
            };

            // Execute the stored procedure
            var employees = await _dbConnection.QueryAsync<Employee>(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );

                var employeeList = employees.ToList();
            int totalCount = employeeList.Count;  // Count employees from the result

            return (employeeList, totalCount);                      
        

        }
        public async Task<List<FAM.Domain.Entities.SubLocation>> GetSublocationByIdAsync(int id)
            {
                const string query = @"
                    SELECT Id, Code, SubLocationName, Description, UnitId, DepartmentId, LocationId, IsActive  
                    FROM FixedAsset.SubLocation 
                    WHERE LocationId = @id AND IsDeleted = 0";

                var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.SubLocation>(query, new { id });

                return result.ToList(); // Convert IEnumerable to List
            }
        //  public async Task<List<Core.Domain.Entities.SubLocation>> GetSublocationByIdAsync(int id)
        // {
           
        //      const string query = @"
        //         SELECT  Id,Code,SubLocationName,Description,UnitId,DepartmentId,LocationId,IsActive  FROM  FixedAsset.SubLocation 
        //         WHERE  LocationId = @id  and  IsDeleted = 0 ";

        //     return  await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.SubLocation>(query, new { id });

        // }     

    }
}