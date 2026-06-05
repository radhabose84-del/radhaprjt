namespace QCManagement.Application.Common.Interfaces.IQcInspection
{
    public interface IQcInspectionCommandRepository
    {
        /// <summary>Creates the inspection header + snapshot detail rows in one save. Returns new header Id.</summary>
        Task<int> CreateAsync(Domain.Entities.QcInspectionHdr entity);

        /// <summary>Updates ActualValue / InspectionResult / Remarks on the supplied detail rows of a header.</summary>
        Task<int> SaveParameterResultsAsync(
            int qcInspectionHdrId,
            IReadOnlyList<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)> results);

        /// <summary>
        /// Saves the parameter readings AND the disposition on the header AND writes the QC result back to
        /// the GRN line — all in one atomic transaction.
        /// </summary>
        Task<int> SaveResultsAndDispositionAsync(
            int qcInspectionHdrId,
            IReadOnlyList<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)> results,
            int qcStatusId, decimal acceptedQty, decimal rejectedQty, string? dispositionRemarks,
            int dispositionByUserId, string? dispositionByName,
            string? qcApprovedIp, bool isQcApproved,
            int grnHeaderId, int grnDetailId);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
