using SalesManagement.Application.StoHeader.Dto;

namespace SalesManagement.Application.StoHeader.Queries.GetPendingStoHeaderById
{
    public class PendingStoHeaderByIdDto
    {
        public int Id { get; set; }
        public string? StoNumber { get; set; }
        public DateOnly DocumentDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }

        // STO Type
        public int StoTypeId { get; set; }
        public string? StoTypeCode { get; set; }
        public string? StoTypeName { get; set; }

        // Movement Type
        public int MovementTypeId { get; set; }
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }

        // Supplying Plant & Storage Location
        public int SupplyingPlantId { get; set; }
        public string? SupplyingPlantName { get; set; }
        public int SupplyingStorageLocationId { get; set; }
        public string? SupplyingStorageLocationName { get; set; }

        // Receiving Plant & Storage Location
        public int ReceivingPlantId { get; set; }
        public string? ReceivingPlantName { get; set; }
        public int ReceivingStorageLocationId { get; set; }
        public string? ReceivingStorageLocationName { get; set; }

        public string? Remarks { get; set; }

        // Header Status
        public int? HeaderStatusId { get; set; }
        public string? HeaderStatusName { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }

        // Detail lines
        public List<StoDetailDto>? StoDetails { get; set; }
    }
}
