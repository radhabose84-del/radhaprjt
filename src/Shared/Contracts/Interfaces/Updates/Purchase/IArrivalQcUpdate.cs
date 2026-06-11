using System.Data;

namespace Contracts.Interfaces.Updates.Purchase
{
    /// <summary>
    /// Cross-module write-back of the QC disposition onto the Arrival header. Arrival QC is
    /// header-level (one sign-off per ArrivalHeader). Accepts the caller's connection +
    /// transaction so the update participates in the QC disposition's atomic transaction.
    /// Mirror of <see cref="IGrnQcUpdate"/>.
    /// </summary>
    public interface IArrivalQcUpdate
    {
        /// <summary>
        /// Writes status, quantities, remarks, person, IP, date and approved flag onto the Arrival header.
        /// <paramref name="arrivalStatusName"/> is the semantic Arrival status the QC module mapped its
        /// disposition to ("Approved" / "Rejected" / "Pending"); this repository resolves the matching
        /// Purchase MiscMaster id (Arrival owns its own status master).
        /// </summary>
        Task UpdateArrivalQcAsync(
            int arrivalHeaderId, string arrivalStatusName,
            decimal acceptedQty, decimal rejectedQty,
            string? qcRemarks, string? qcPersonName, string? qcApprovedIp,
            DateTimeOffset whenUtc, bool isQcApproved,
            IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Sets ONLY the Arrival header's <c>QcStatusId</c> to the given QC.MiscMaster id
        /// (no disposition quantities/remarks). Used when a QC inspection is first created against an
        /// Arrival, so the header reflects "Pending QC" immediately. The caller (QC module) resolves
        /// the id from QC.MiscMaster. Manages its own connection (not part of an external transaction).
        /// </summary>
        Task SetArrivalQcStatusAsync(int arrivalHeaderId, int qcStatusId, CancellationToken ct = default);
    }
}
