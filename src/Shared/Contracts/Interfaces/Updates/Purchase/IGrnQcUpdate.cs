using System.Data;

namespace Contracts.Interfaces.Updates.Purchase
{
    /// <summary>
    /// Cross-module write-back of QC results onto GRN tables, owned by PurchaseManagement.
    /// Methods accept the caller's connection + transaction so the GRN update participates
    /// in the QC disposition's atomic transaction.
    /// </summary>
    public interface IGrnQcUpdate
    {
        /// <summary>Writes accepted/rejected quantities and rejection remarks onto the GRN line.</summary>
        Task UpdateGrnDetailQcQuantitiesAsync(
            int grnDetailId, decimal acceptedQty, decimal rejectedQty, string? rejectionRemarks,
            IDbConnection connection, IDbTransaction transaction);

        /// <summary>Stamps QcDate + QcPersonName on the GRN header. Does NOT set QcStatusId/IsQcApproved (GRN-level aggregates).</summary>
        Task StampGrnHeaderQcAsync(
            int grnHeaderId, string inspectorName, DateTimeOffset whenUtc,
            IDbConnection connection, IDbTransaction transaction);
    }
}
