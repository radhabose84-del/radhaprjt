namespace PurchaseManagement.Application.OCREntry.Dto
{
    public class OCREntryDto
    {
        public int Id { get; set; }
        public string? OcrNumber { get; set; }
        public DateTimeOffset OcrDate { get; set; }

        // Same-module FKs (names via Dapper JOIN)
        public int ProcurementSourceId { get; set; }
        public string? ProcurementSourceName { get; set; }

        public int ProcurementTypeId { get; set; }
        public string? ProcurementTypeName { get; set; }

        public int BrokerDirectId { get; set; }
        public string? BrokerDirectName { get; set; }

        public string? BrokerName { get; set; }

        public int? GradeId { get; set; }
        public string? GradeName { get; set; }

        public int PaymentTermId { get; set; }
        public string? PaymentTermName { get; set; }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        // Cross-module FKs (names via lookup)
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public int LocationId { get; set; }
        public string? LocationName { get; set; }

        public int StationId { get; set; }
        public string? StationName { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }

        public int CountId { get; set; }
        public string? CountName { get; set; }

        // Cotton commercial details
        public decimal Quantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal Rate { get; set; }
        public DateTimeOffset? ExpectedDispatchDate { get; set; }
        public string? DocumentPath { get; set; }

        // Full retrievable path = OCRPath (MiscMaster) + company/unit + DocumentPath. Populated on GetById.
        public string? DocumentFullPath { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
