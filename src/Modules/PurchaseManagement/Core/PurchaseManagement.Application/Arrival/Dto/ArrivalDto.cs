namespace PurchaseManagement.Application.Arrival.Dto
{
    public class ArrivalDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? ArrivalNumber { get; set; }
        public DateTimeOffset ArrivalDate { get; set; }

        // Raw Material PO reference (same-module JOIN)
        public int RawMaterialPOId { get; set; }
        public string? PONumber { get; set; }

        public string? VehicleNumber { get; set; }

        // Cross-module FK names (populated via lookup)
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public int StationId { get; set; }
        public string? StationName { get; set; }

        public int GodownId { get; set; }
        public string? GodownName { get; set; }

        public int TransporterId { get; set; }
        public string? TransporterName { get; set; }

        public decimal? FreightRate { get; set; }
        public string? InvoiceGstNo { get; set; }
        public string? LrNumber { get; set; }
        public string? ContainerNo { get; set; }

        public DateTimeOffset? LorryIn { get; set; }
        public DateTimeOffset? LorryOut { get; set; }

        // Weighbridge
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal PartyWeight { get; set; }
        public decimal WeightDifference { get; set; }
        public decimal? MoisturePercentage { get; set; }

        public int? QcStatusId { get; set; }
        public string? QcStatusName { get; set; }

        // QC.MiscMaster Id for QP_SOURCE_TYPE / code 'ARRIVAL' — resolved via IQcMiscMasterLookup.
        public int? SourceTypeId { get; set; }

        // Edit is blocked once QC is signed off (QcStatusId set). Computed — no DB column.
        public bool IsEditAllowed => !QcStatusId.HasValue;

        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }

        public List<ArrivalDetailDto> Details { get; set; } = new();
    }
}
