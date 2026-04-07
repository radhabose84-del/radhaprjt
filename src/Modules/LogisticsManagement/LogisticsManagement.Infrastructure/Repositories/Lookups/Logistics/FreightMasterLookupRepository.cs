using System.Data;
using Contracts.Dtos.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Logistics;
using Dapper;

namespace LogisticsManagement.Infrastructure.Repositories.Lookups.Logistics
{
    internal sealed class FreightMasterLookupRepository : IFreightMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public FreightMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<FreightMasterLookupDto>> GetAllFreightMasterAsync()
        {
            const string sql = @"
                SELECT fm.Id, fm.FreightModeId, fmode.Description AS FreightModeName,
                       fm.RateMethodId, fmethod.Description AS RateMethodName,
                       fm.Rate, fm.ModuleId
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster fmode ON fm.FreightModeId = fmode.Id AND fmode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster fmethod ON fm.RateMethodId = fmethod.Id AND fmethod.IsDeleted = 0
                WHERE fm.IsActive = 1 AND fm.IsDeleted = 0
                ORDER BY fmode.Description, fmethod.Description";

            var result = await _dbConnection.QueryAsync<FreightMasterLookupDto>(sql);
            return result.ToList();
        }

        public async Task<List<FreightMasterLookupDto>> GetByModuleIdAsync(int moduleId)
        {
            const string sql = @"
                SELECT fm.Id, fm.FreightModeId, fmode.Description AS FreightModeName,
                       fm.RateMethodId, fmethod.Description AS RateMethodName,
                       fm.Rate, fm.ModuleId
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster fmode ON fm.FreightModeId = fmode.Id AND fmode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster fmethod ON fm.RateMethodId = fmethod.Id AND fmethod.IsDeleted = 0
                WHERE fm.IsActive = 1 AND fm.IsDeleted = 0 AND fm.ModuleId = @ModuleId
                ORDER BY fmode.Description, fmethod.Description";

            var result = await _dbConnection.QueryAsync<FreightMasterLookupDto>(sql, new { ModuleId = moduleId });
            return result.ToList();
        }

        public async Task<FreightMasterLookupDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT fm.Id, fm.FreightModeId, fmode.Description AS FreightModeName,
                       fm.RateMethodId, fmethod.Description AS RateMethodName,
                       fm.Rate, fm.ModuleId
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster fmode ON fm.FreightModeId = fmode.Id AND fmode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster fmethod ON fm.RateMethodId = fmethod.Id AND fmethod.IsDeleted = 0
                WHERE fm.Id = @Id AND fm.IsActive = 1 AND fm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<FreightMasterLookupDto>(sql, new { Id = id });
        }
    }
}
