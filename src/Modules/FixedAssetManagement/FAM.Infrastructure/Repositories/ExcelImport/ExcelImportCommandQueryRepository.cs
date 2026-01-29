using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport;
using Dapper;
using FAM.Application.ExcelImport;

namespace FAM.Infrastructure.Repositories.ExcelImport
{
    public class ExcelImportCommandQueryRepository  : IExcelImportQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public ExcelImportCommandQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<int?> GetAssetDeptIdByNameAsync(string deptName)
        {
            const string query = @"            
             SELECT  Id from Bannari.appData.Department where DeptName like @deptName and IsDeleted=0";            
            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { deptName = $"%{deptName}%" });
            return result;
        }

         public async Task<int?> GetAssetUnitIdByNameAsync(string unitName)
        {
            const string query = @"            
                SELECT Id 
                FROM Bannari.appData.Unit 
                WHERE UnitName LIKE @unitName AND IsDeleted = 0";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { unitName = $"%{unitName}%" });
            return result;
        }


        public async Task<string?> GetCompanyByNameAsync(int companyId)
        {
            const string query = @"            
             SELECT  CompanyName from Bannari.appData.Company where id =@companyId and IsDeleted=0";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<string?>(query, new { companyId});
            return result;
        }

         public async Task<UnitDto?> GetUnitByNameAsync(int unitId)
        {
            const string query = @"            
            SELECT shortname, OldUnitId 
            FROM Bannari.appData.Unit 
            WHERE id = @unitId AND IsDeleted = 0";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<UnitDto>(query, new { unitId });
            return result;
        }

    }
}