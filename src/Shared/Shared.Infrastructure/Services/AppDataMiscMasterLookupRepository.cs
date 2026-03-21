using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Common;
using Dapper;

namespace Shared.Infrastructure.Services
{
    internal sealed class AppDataMiscMasterLookupRepository : IAppDataMiscMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public AppDataMiscMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<MiscMasterLookupDto?> GetMiscMasterByNameAsync(string miscTypeCode, string code)
        {
            const string sql = @"
                SELECT M.Id, M.Code, M.Description
                FROM AppData.MiscMaster AS M
                INNER JOIN AppData.MiscTypeMaster AS MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND M.IsActive = 1
                AND LOWER(MT.MiscTypeCode) = LOWER(@MiscTypeCode)
                AND LOWER(M.Code) = LOWER(@MiscTypeName)";

            return await _dbConnection.QueryFirstOrDefaultAsync<MiscMasterLookupDto>(
                sql,
                new { MiscTypeCode = miscTypeCode, MiscTypeName = code });
        }
    }
}
