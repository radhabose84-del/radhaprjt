using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.Common.PeriodStatus;
using FinanceManagement.Application.PeriodStatusOverride.Dto;

namespace FinanceManagement.Infrastructure.Repositories.PeriodStatusOverride
{
    public class PeriodStatusOverrideQueryRepository : IPeriodStatusOverrideQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public PeriodStatusOverrideQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private const string PeriodStatusSelect = @"
            fp.Id AS PeriodId, fp.FinancialYearId, fy.FinancialYearCode, fp.CompanyId,
            fp.PeriodNumber, fp.PeriodName, fp.StartDate, fp.EndDate,
            fp.StatusId, fps.Code AS StatusCode, fps.Description AS StatusName,
            fp.IsAdjustmentPeriod, fp.LastStatusChangedBy, fp.LastStatusChangedAt
            FROM Finance.FinancialPeriodMaster fp
            LEFT JOIN Finance.FinancialYearMaster fy ON fp.FinancialYearId = fy.Id AND fy.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster fps ON fp.StatusId = fps.Id AND fps.IsDeleted = 0
        ";

        private const string OverrideSelect = @"
            pso.Id, pso.FinancialPeriodId, fy.FinancialYearCode, fp.PeriodName, pso.CompanyId,
            pso.FromStatusId,    frs.Code  AS FromStatusCode,    frs.Description  AS FromStatusName,
            pso.ToStatusId,      tos.Code  AS ToStatusCode,      tos.Description  AS ToStatusName,
            pso.RequestedBy, pso.RequestedAt, pso.RequestedReason,
            pso.CfoApproverId, pso.CfoApprovedAt,
            pso.SysAdminApproverId, pso.SysAdminApprovedAt,
            pso.OverrideStatusId, ost.Code AS OverrideStatusCode, ost.Description AS OverrideStatusName,
            pso.AppliedAt, pso.RejectionReason,
            pso.IsActive, pso.IsDeleted,
            pso.CreatedBy, pso.CreatedDate, pso.CreatedByName,
            pso.ModifiedBy, pso.ModifiedDate, pso.ModifiedByName
            FROM Finance.PeriodStatusOverride pso
            LEFT JOIN Finance.FinancialPeriodMaster fp ON pso.FinancialPeriodId = fp.Id AND fp.IsDeleted = 0
            LEFT JOIN Finance.FinancialYearMaster fy ON fp.FinancialYearId = fy.Id AND fy.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster frs ON pso.FromStatusId     = frs.Id AND frs.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster tos ON pso.ToStatusId       = tos.Id AND tos.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster ost ON pso.OverrideStatusId = ost.Id AND ost.IsDeleted = 0
        ";

        public async Task<FinancialPeriodStatusDto?> GetPeriodStatusAsync(int periodId, int companyId, CancellationToken ct)
        {
            var sql = $@"
                SELECT {PeriodStatusSelect}
                WHERE fp.Id = @Id AND fp.CompanyId = @CompanyId AND fp.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<FinancialPeriodStatusDto>(
                new CommandDefinition(sql, new { Id = periodId, CompanyId = companyId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<PeriodStatusOverrideDto>> GetHistoryForPeriodAsync(int periodId, int companyId, CancellationToken ct)
        {
            var sql = $@"
                SELECT {OverrideSelect}
                WHERE pso.FinancialPeriodId = @Id AND pso.CompanyId = @CompanyId AND pso.IsDeleted = 0
                ORDER BY pso.RequestedAt DESC, pso.Id DESC";

            var result = await _dbConnection.QueryAsync<PeriodStatusOverrideDto>(
                new CommandDefinition(sql, new { Id = periodId, CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<IReadOnlyList<PeriodStatusOverrideDto>> GetPendingForCompanyAsync(int companyId, CancellationToken ct)
        {
            var sql = $@"
                SELECT {OverrideSelect}
                WHERE pso.CompanyId = @CompanyId AND pso.IsDeleted = 0
                  AND ost.Code = @Pending
                ORDER BY pso.RequestedAt ASC";

            var result = await _dbConnection.QueryAsync<PeriodStatusOverrideDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, Pending = PeriodStatusConstants.OverridePending }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<PeriodStatusOverrideDto?> GetByIdAsync(int id)
        {
            var sql = $@"
                SELECT {OverrideSelect}
                WHERE pso.Id = @Id AND pso.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<PeriodStatusOverrideDto>(sql, new { Id = id });
        }

        public async Task<bool> HasOpenOverrideAsync(int periodId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.PeriodStatusOverride pso
                INNER JOIN Finance.MiscMaster ost ON pso.OverrideStatusId = ost.Id AND ost.IsDeleted = 0
                WHERE pso.FinancialPeriodId = @Id AND pso.IsDeleted = 0
                  AND ost.Code IN (@Pending, @FullyApproved)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                Id            = periodId,
                Pending       = PeriodStatusConstants.OverridePending,
                FullyApproved = PeriodStatusConstants.OverrideFullyApproved
            });
            return count > 0;
        }

        public async Task<int> GetMiscMasterIdByCodeAsync(string miscTypeCode, string valueCode)
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mtm.MiscTypeCode = @TypeCode
                  AND mm.Code = @ValueCode
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeCode = miscTypeCode, ValueCode = valueCode });
        }

        public async Task<PeriodSnapshotDto?> GetPeriodSnapshotAsync(int periodId, CancellationToken ct)
        {
            const string sql = @"
                SELECT fp.Id, fp.CompanyId, fp.FinancialYearId, fp.StatusId,
                       fps.Code AS StatusCode, fp.IsAdjustmentPeriod
                FROM Finance.FinancialPeriodMaster fp
                LEFT JOIN Finance.MiscMaster fps ON fp.StatusId = fps.Id AND fps.IsDeleted = 0
                WHERE fp.Id = @Id AND fp.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<PeriodSnapshotDto>(
                new CommandDefinition(sql, new { Id = periodId }, cancellationToken: ct));
        }
    }
}
