using System.Data;
using Contracts.Interfaces.Updates.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Updates.Purchase
{
    internal sealed class ArrivalQcUpdateRepository : IArrivalQcUpdate
    {
        // Arrival QC status master lives under MiscType "ApprovalStatus" (Pending / Approved / Rejected).
        private const string ArrivalStatusTypeCode = "ApprovalStatus";

        private readonly IDbConnection _connection;

        public ArrivalQcUpdateRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task SetArrivalQcStatusAsync(int arrivalHeaderId, int qcStatusId, CancellationToken ct = default)
        {
            // QcStatusId is a cross-module reference to QC.MiscMaster — the QC module resolved the id,
            // so this only writes it onto the header (no Purchase-side status resolution).
            const string updateSql = @"
                UPDATE Purchase.ArrivalHeader
                SET QcStatusId = @QcStatusId
                WHERE Id = @ArrivalHeaderId;";

            await _connection.ExecuteAsync(new CommandDefinition(
                updateSql,
                new { QcStatusId = qcStatusId, ArrivalHeaderId = arrivalHeaderId },
                cancellationToken: ct));
        }

        public async Task UpdateArrivalQcAsync(
            int arrivalHeaderId, string arrivalStatusName,
            decimal acceptedQty, decimal rejectedQty,
            string? qcRemarks, string? qcPersonName, string? qcApprovedIp,
            DateTimeOffset whenUtc, bool isQcApproved,
            IDbConnection connection, IDbTransaction transaction)
        {
            // Resolve Arrival's own status id from the mapped semantic name (Arrival owns its status master).
            const string resolveSql = @"
                SELECT TOP 1 mm.Id
                FROM Purchase.MiscMaster mm
                INNER JOIN Purchase.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE mt.MiscTypeCode = @TypeCode
                  AND (mm.Code = @StatusName OR mm.Description = @StatusName)
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                ORDER BY mm.Id ASC;";

            var statusId = await connection.ExecuteScalarAsync<int?>(
                resolveSql,
                new { TypeCode = ArrivalStatusTypeCode, StatusName = arrivalStatusName },
                transaction);

            // QC is header-level on ArrivalHeader (mirrors GrnDetail columns). Located by primary key.
            const string updateSql = @"
                UPDATE Purchase.ArrivalHeader
                SET QcStatusId         = ISNULL(@QcStatusId, QcStatusId),
                    QcAcceptedQuantity = @AcceptedQty,
                    QcRejectedQuantity = @RejectedQty,
                    QcRemarks          = @QcRemarks,
                    QcPersonName       = @QcPersonName,
                    QcDate             = @WhenUtc,
                    QcApprovedIp       = @QcApprovedIp,
                    IsQcApproved       = @IsQcApproved
                WHERE Id = @ArrivalHeaderId;";

            await connection.ExecuteAsync(
                updateSql,
                new
                {
                    ArrivalHeaderId = arrivalHeaderId,
                    QcStatusId = statusId,
                    AcceptedQty = acceptedQty,
                    RejectedQty = rejectedQty,
                    QcRemarks = qcRemarks,
                    QcPersonName = qcPersonName,
                    WhenUtc = whenUtc,
                    QcApprovedIp = qcApprovedIp,
                    IsQcApproved = isQcApproved
                },
                transaction);
        }
    }
}
