using System.Data;
using Contracts.Interfaces.Updates.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Updates.Purchase
{
    internal sealed class GrnQcUpdateRepository : IGrnQcUpdate
    {
        public async Task UpdateGrnDetailQcAsync(
            int grnDetailId, int qcStatusId,
            decimal acceptedQty, decimal rejectedQty,
            string? qcRemarks, string? qcPersonName, string? qcApprovedIp,
            DateTimeOffset whenUtc, bool isQcApproved,
            IDbConnection connection, IDbTransaction transaction)
        {
            // QC is line-level: the disposition is written onto GrnDetail (GrnHeader no longer has QC columns).
            // Row located by primary key (Id); GrnId/PoId/PoSlNoLocal are reference-only and never changed.
            // QcRejectedRemarks is intentionally not written (no dedicated input yet).
            const string sql = @"
                UPDATE Purchase.GrnDetail
                SET QcStatusId         = @QcStatusId,
                    QcAcceptedQuantity = @AcceptedQty,
                    QcRejectedQuantity = @RejectedQty,
                    QcRemarks          = @QcRemarks,
                    QcPersonName       = @QcPersonName,
                    QcDate             = @WhenUtc,
                    QcApprovedIp       = @QcApprovedIp,
                    IsQcApproved       = @IsQcApproved
                WHERE Id = @GrnDetailId;";

            await connection.ExecuteAsync(
                sql,
                new
                {
                    GrnDetailId = grnDetailId,
                    QcStatusId = qcStatusId,
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
