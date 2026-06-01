using System.Data;
using Contracts.Interfaces.Updates.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Updates.Purchase
{
    internal sealed class GrnQcUpdateRepository : IGrnQcUpdate
    {
        public async Task UpdateGrnDetailQcQuantitiesAsync(
            int grnDetailId, decimal acceptedQty, decimal rejectedQty, string? rejectionRemarks,
            IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = @"
                UPDATE Purchase.GrnDetail
                SET QcAcceptedQuantity = @AcceptedQty,
                    QcRejectedQuantity = @RejectedQty,
                    QcRejectedRemarks  = @RejectionRemarks
                WHERE Id = @GrnDetailId;";

            await connection.ExecuteAsync(
                sql,
                new { GrnDetailId = grnDetailId, AcceptedQty = acceptedQty, RejectedQty = rejectedQty, RejectionRemarks = rejectionRemarks },
                transaction);
        }

        public async Task StampGrnHeaderQcAsync(
            int grnHeaderId, string inspectorName, DateTimeOffset whenUtc,
            IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = @"
                UPDATE Purchase.GrnHeader
                SET QcDate       = @WhenUtc,
                    QcPersonName = @InspectorName
                WHERE Id = @GrnHeaderId;";

            await connection.ExecuteAsync(
                sql,
                new { GrnHeaderId = grnHeaderId, WhenUtc = whenUtc, InspectorName = inspectorName },
                transaction);
        }
    }
}
