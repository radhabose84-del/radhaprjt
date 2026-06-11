using System.Data;
using Contracts.Dtos.Lookups.Gate;
using Contracts.Interfaces.Lookups.Gate;
using Dapper;

namespace GateEntryManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Reads <c>Gate.VehicleMovementRecord</c> rows by id, returning the display fields other
    /// modules (e.g. PurchaseManagement's Arrival) need without a cross-schema JOIN. Soft-deleted
    /// rows are excluded. Auto-cached by the global AddLookupCaching wrapper (name ends with "Lookup").
    /// </summary>
    internal sealed class VehicleMovementRecordLookupRepository : IVehicleMovementRecordLookup
    {
        private readonly IDbConnection _dbConnection;

        public VehicleMovementRecordLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<VehicleMovementRecordLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (idList.Count == 0)
                return Array.Empty<VehicleMovementRecordLookupDto>();

            const string sql = @"
                SELECT
                    Id,
                    VehicleMovementId,
                    VehicleNumber,
                    DriverName,
                    DriverMobileNo,
                    TransporterId,
                    ReferenceDocNo,
                    GateInTime,
                    GateOutTime,
                    StatusId
                FROM Gate.VehicleMovementRecord
                WHERE Id IN @Ids
                  AND IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<VehicleMovementRecordLookupDto>(
                new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct));
            return rows.ToList();
        }
    }
}
