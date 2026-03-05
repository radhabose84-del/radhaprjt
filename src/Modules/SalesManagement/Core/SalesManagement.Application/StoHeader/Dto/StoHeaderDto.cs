namespace SalesManagement.Application.StoHeader.Dto
{
    public class StoHeaderDto
    {
        public int Id { get; set; }
        public string? StoNumber { get; set; }
        public DateOnly DocumentDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }

        // STO Type (same-module JOIN)
        public int StoTypeId { get; set; }
        public string? StoTypeCode { get; set; }
        public string? StoTypeName { get; set; }

        // Movement Type (same-module JOIN)
        public int MovementTypeId { get; set; }
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }

        // Supplying Plant & Storage Location (cross-module lookup)
        public int SupplyingPlantId { get; set; }
        public string? SupplyingPlantName { get; set; }
        public int SupplyingStorageLocationId { get; set; }
        public string? SupplyingStorageLocationName { get; set; }

        // Receiving Plant & Storage Location (cross-module lookup)
        public int ReceivingPlantId { get; set; }
        public string? ReceivingPlantName { get; set; }
        public int ReceivingStorageLocationId { get; set; }
        public string? ReceivingStorageLocationName { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

        // Child details (populated only in GetById, null in GetAll)
        public List<StoDetailDto>? StoDetails { get; set; }
    }
}
