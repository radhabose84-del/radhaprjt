using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.Common.Interfaces.IQcInspection
{
    public interface IQcInspectionQueryRepository
    {
        Task<QcInspectionDto?> GetByIdAsync(int id);

        /// <summary>Inspection summaries for a set of source detail lines (per source type) — used to build the unified grid.</summary>
        Task<IReadOnlyList<QcInspectionSummaryDto>> GetInspectionSummariesBySourceAsync(int sourceTypeId, IEnumerable<int> sourceDetailIds);

        /// <summary>Resolves the QC.MiscMaster Id for a QP_SOURCE_TYPE code (GRN / ARRIVAL).</summary>
        Task<int?> GetSourceTypeIdByCodeAsync(string sourceTypeCode);

        // ── validation / existence helpers ──
        Task<bool> NotFoundAsync(int id);
        Task<bool> InspectionExistsForSourceAsync(int sourceTypeId, int sourceDetailId);
        Task<bool> IsDisposedAsync(int id);
        Task<bool> AllParametersEvaluatedAsync(int id);
        Task<bool> HasCriticalFailureAsync(int id);
        Task<decimal?> GetReceivedQuantityAsync(int id);
        Task<bool> DetailBelongsToHeaderAsync(int detailId, int headerId);
        Task<bool> QcStatusIdExistsAsync(int qcStatusId);

        /// <summary>Resolves the QC.MiscMaster Id for a QP_QC_STATUS code (e.g. "PENDING").</summary>
        Task<int?> GetQcStatusIdByCodeAsync(string statusCode);

        // ── create-time resolution ──
        Task<int> GetMaxInspectionSequenceAsync(int year);
        Task<int?> GetPurchasedGoodsQcTypeIdAsync();
        Task<int?> ResolveActiveSpecIdAsync(int itemId, int? itemCategoryId, int qcTypeId, DateTimeOffset asOfDate);
        Task<QcSpecSnapshotDto?> GetSpecSnapshotAsync(int qualitySpecificationId);

        // ── disposition support ──
        Task<string?> GetQcStatusCodeByIdAsync(int qcStatusId);
        Task<IReadOnlyList<QcInspectionDtlEvalDto>> GetDetailEvaluationRowsAsync(int qcInspectionHdrId);
        Task<QcDispositionContextDto?> GetDispositionContextAsync(int id);

        // ── GRN-level derivation ──
        /// <summary>QC-side disposition counts for a GRN header (TotalLines + DerivedStatus are filled by the handler).</summary>
        Task<GrnQcStatusDto> GetGrnStatusCountsAsync(int grnHeaderId);
    }
}
