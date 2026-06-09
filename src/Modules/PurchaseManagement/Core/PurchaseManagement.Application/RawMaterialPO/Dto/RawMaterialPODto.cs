namespace PurchaseManagement.Application.RawMaterialPO.Dto
{
    public class RawMaterialPODto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? PONumber { get; set; }
        public DateTimeOffset PODate { get; set; }

        // OCR reference (same-module JOIN)
        public int OcrId { get; set; }
        public string? OcrNumber { get; set; }

        public int ProcurementDocumentTypeId { get; set; }
        public string? ProcurementDocumentTypeName { get; set; }

        // Conversion progress (auto-calculated)
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public decimal? TaxableTotal { get; set; }
        public decimal? TotalGstAmount { get; set; }
        public decimal? NetTotal { get; set; }

        public string? Remarks { get; set; }

        // Additional cotton details
        public string? CropYear { get; set; }
        public string? ArrivalType { get; set; }
        public DateTimeOffset? PassingDate { get; set; }
        public int? CreditDays { get; set; }
        public string? CottonApprovedBy { get; set; }
        public DateTimeOffset? CottonApprovedOn { get; set; }

        // Stored bare file name + full retrievable URL (URL populated on GetById)
        public string? DocumentPath { get; set; }
        public string? DocumentFullPath { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // True when an Arrival has been created against this PO.
        // When true the PO is locked: CanEdit = CanDelete = false.
        public bool IsArrivalCreated { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }

        public List<RawMaterialPODetailDto> Details { get; set; } = new();
    }
}
