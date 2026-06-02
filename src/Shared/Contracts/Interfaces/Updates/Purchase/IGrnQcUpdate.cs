using System.Data;

namespace Contracts.Interfaces.Updates.Purchase
{
    /// <summary>
    /// Cross-module write-back of the QC disposition onto the GRN line. QC is line-level on GrnDetail
    /// (QC columns were moved off GrnHeader). Accepts the caller's connection + transaction so the
    /// update participates in the QC disposition's atomic transaction.
    /// </summary>
    public interface IGrnQcUpdate
    {
        /// <summary>Writes status, quantities, remarks, person, IP, date and approved flag onto the GRN line.</summary>
        Task UpdateGrnDetailQcAsync(
            int grnDetailId, int qcStatusId,
            decimal acceptedQty, decimal rejectedQty,
            string? qcRemarks, string? qcPersonName, string? qcApprovedIp,
            DateTimeOffset whenUtc, bool isQcApproved,
            IDbConnection connection, IDbTransaction transaction);
    }
}
