namespace QCManagement.Application.QcInspection.Dto
{
    /// <summary>
    /// Unified grid row — one per QC-required GRN line. A line with no inspection yet
    /// is "Pending QC" (InspectionId / QcInspectionNo null); an inspected line carries its status.
    /// Status text for pending rows is set by the query handler, not defaulted here.
    /// </summary>
    public class QcInspectionListDto
    {
        public int? InspectionId { get; set; }
        public string? QcInspectionNo { get; set; }
        public int SourceTypeId { get; set; }
        public string? SourceTypeCode { get; set; }   // GRN / ARRIVAL
        public string? SourceTypeName { get; set; }
        public int SourceHeaderId { get; set; }
        public int SourceDetailId { get; set; }
        public string? SourceNo { get; set; }          // GrnNo or ArrivalNumber
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? BatchNumber { get; set; }
        public decimal ReceivedQuantity { get; set; }

        // "Y"/"N" QC template availability — the item (or its category) has an active Qc.QualitySpecification.
        public string? IsTemplateAvailable { get; set; }
        public decimal? AcceptedQuantity { get; set; }
        public decimal? RejectedQuantity { get; set; }
        public int? QcStatusId { get; set; }
        public string? QcStatusCode { get; set; }
        public string? QcStatusName { get; set; }
        public DateTimeOffset? InspectionDate { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
