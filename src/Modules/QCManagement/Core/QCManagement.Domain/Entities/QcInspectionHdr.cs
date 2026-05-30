using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QcInspectionHdr : BaseEntity, IActivityTracked
    {
        public string? QcInspectionNo { get; set; }
        public DateTimeOffset InspectionDate { get; set; }

        // GRN source (cross-module — no DB FK constraint)
        public int GrnHeaderId { get; set; }
        public int GrnDetailId { get; set; }

        // Quality specification snapshot (snapshotted — no DB FK constraint, survives spec changes)
        public int QualitySpecificationId { get; set; }
        public string? QualitySpecificationCode { get; set; }
        public int QualityTemplateId { get; set; }
        public string? QualityTemplateCode { get; set; }
        public int QcTypeId { get; set; }

        // Inspector
        public int InspectorUserId { get; set; }
        public string? InspectorName { get; set; }

        // Received snapshot (from GrnDetail at create-time)
        public decimal ReceivedQuantity { get; set; }
        public int ReceivedUomId { get; set; }
        public string? BatchNumber { get; set; }
        public string? LotNumber { get; set; }

        // Disposition (NULL until disposition saved)
        public int? QcStatusId { get; set; }
        public decimal? AcceptedQuantity { get; set; }
        public decimal? RejectedQuantity { get; set; }
        public string? DispositionRemarks { get; set; }
        public DateTimeOffset? DispositionDate { get; set; }
        public int? DispositionByUserId { get; set; }
        public string? DispositionByName { get; set; }

        // Same-module FK navigation (QcStatusId → QC.MiscMaster)
        public MiscMaster? QcStatus { get; set; }

        // Child parameter rows
        public ICollection<QcInspectionDtl>? Details { get; set; }
    }
}
