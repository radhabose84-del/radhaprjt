using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.Common.Interfaces.IQcInspection
{
    public interface IQcInspectionQueryRepository
    {
        Task<QcInspectionDto?> GetByIdAsync(int id);

        /// <summary>Inspection summaries for a set of GRN detail lines — used to build the unified grid.</summary>
        Task<IReadOnlyList<QcInspectionSummaryDto>> GetInspectionSummariesByGrnDetailIdsAsync(IEnumerable<int> grnDetailIds);

        // ── validation / existence helpers ──
        Task<bool> NotFoundAsync(int id);
        Task<bool> InspectionExistsForGrnDetailAsync(int grnDetailId);
        Task<bool> IsDisposedAsync(int id);
        Task<bool> AllParametersEvaluatedAsync(int id);
        Task<bool> HasCriticalFailureAsync(int id);
        Task<decimal?> GetReceivedQuantityAsync(int id);
        Task<bool> DetailBelongsToHeaderAsync(int detailId, int headerId);
        Task<bool> QcStatusCodeExistsAsync(string qcStatusCode);

        // ── create-time resolution ──
        Task<int> GetMaxInspectionSequenceAsync(int year);
        Task<int?> GetPurchasedGoodsQcTypeIdAsync();
        Task<int?> ResolveActiveSpecIdAsync(int itemId, int? itemCategoryId, int qcTypeId, DateTimeOffset asOfDate);
        Task<QcSpecSnapshotDto?> GetSpecSnapshotAsync(int qualitySpecificationId);

        // ── disposition support ──
        Task<int?> GetQcStatusIdByCodeAsync(string qcStatusCode);
        Task<IReadOnlyList<QcInspectionDtlEvalDto>> GetDetailEvaluationRowsAsync(int qcInspectionHdrId);
        Task<QcDispositionContextDto?> GetDispositionContextAsync(int id);

        // ── GRN-level derivation ──
        /// <summary>QC-side disposition counts for a GRN header (TotalLines + DerivedStatus are filled by the handler).</summary>
        Task<GrnQcStatusDto> GetGrnStatusCountsAsync(int grnHeaderId);
    }
}
