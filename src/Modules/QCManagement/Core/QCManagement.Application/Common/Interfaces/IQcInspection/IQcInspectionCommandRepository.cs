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
        /// Persists disposition on the header AND writes QC results back to the GRN tables in one atomic transaction.
        /// </summary>
        Task<int> SaveDispositionAsync(
            int qcInspectionHdrId, int qcStatusId,
            decimal acceptedQty, decimal rejectedQty, string? dispositionRemarks,
            int dispositionByUserId, string? dispositionByName,
            int grnHeaderId, int grnDetailId);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
