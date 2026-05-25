using System.Data;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Maintenance;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance
{
    internal class MaintenanceRequestLookupRepository : IMaintenanceRequestLookup
    {
        private readonly IDbConnection _dbConnection;

        public MaintenanceRequestLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Shared SELECT — keeps column mapping consistent across the three methods.
        private const string SelectClause = @"
            mr.Id,
            CONCAT('MR-', mr.Id) AS RequestNo,
            mr.MachineId,
            mm.MachineName,
            mr.MaintenanceDepartmentId,
            mr.MaintenanceTypeId,
            mt.description AS MaintenanceTypeName,
            mr.ServiceTypeId,
            st.description AS ServiceTypeName,
            mr.VendorId,
            mr.VendorName,
            mr.EstimatedServiceCost,
            mr.ConvertedToPoAmount,
            mr.Remarks,
            mr.RequestStatusId,
            rs.Code AS RequestStatusCode";

        private const string FromJoins = @"
            FROM [Maintenance].[MaintenanceRequest] mr
            INNER JOIN [Maintenance].[MiscMaster] rt ON mr.RequestTypeId = rt.Id AND rt.IsDeleted = 0
            LEFT JOIN  [Maintenance].[MachineMaster] mm ON mr.MachineId = mm.Id AND mm.IsDeleted = 0
            LEFT JOIN  [Maintenance].[MiscMaster] mt ON mr.MaintenanceTypeId = mt.Id AND mt.IsDeleted = 0
            LEFT JOIN  [Maintenance].[MiscMaster] st ON mr.ServiceTypeId = st.Id AND st.IsDeleted = 0
            LEFT JOIN  [Maintenance].[MiscMaster] rs ON mr.RequestStatusId = rs.Id AND rs.IsDeleted = 0";

        public async Task<IReadOnlyList<MaintenanceRequestLookupDto>> GetAvailableForServicePoAsync(
            string? searchTerm, CancellationToken ct = default)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (CAST(mr.Id AS varchar(20)) LIKE @Term
                       OR mr.VendorName LIKE @Term
                       OR mr.Remarks LIKE @Term
                       OR mm.MachineName LIKE @Term)";

            var sql = $@"
                SELECT TOP 100 {SelectClause}
                {FromJoins}
                WHERE mr.IsDeleted = 0
                  AND mr.IsActive = 1
                  AND rt.Code = 'External'
                  AND rs.Code IN ('Open', 'InProgress', 'PartiallyConverted')
                  {searchFilter}
                ORDER BY mr.Id DESC;";

            var rows = await _dbConnection.QueryAsync<MaintenanceRequestLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{searchTerm}%" }, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<MaintenanceRequestLookupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT TOP 1 {SelectClause}
                {FromJoins}
                WHERE mr.IsDeleted = 0 AND mr.Id = @Id;";

            return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceRequestLookupDto>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<MaintenanceRequestLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idArray = ids?.Where(i => i > 0).Distinct().ToArray() ?? Array.Empty<int>();
            if (idArray.Length == 0)
                return Array.Empty<MaintenanceRequestLookupDto>();

            var sql = $@"
                SELECT {SelectClause}
                {FromJoins}
                WHERE mr.IsDeleted = 0 AND mr.Id IN @Ids;";

            var rows = await _dbConnection.QueryAsync<MaintenanceRequestLookupDto>(
                new CommandDefinition(sql, new { Ids = idArray }, cancellationToken: ct));
            return rows.ToList();
        }
    }
}
