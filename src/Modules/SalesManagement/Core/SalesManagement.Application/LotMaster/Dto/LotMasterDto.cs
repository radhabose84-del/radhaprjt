namespace SalesManagement.Application.LotMaster.Dto
{
    public class LotMasterDto
    {
        public int Id { get; set; }
        public string? LotCode { get; set; }
        public string? BatchNumber { get; set; }

        public int LotTypeId { get; set; }
        public string? LotTypeName { get; set; }

        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public int UnitId { get; set; }
        public string? UnitName { get; set; }

        public DateOnly StartDate { get; set; }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public string? ProductionOrderRef { get; set; }
        public decimal TotalProducedQty { get; set; }
        public decimal AvailableQty { get; set; }
        public DateOnly? RunOutDate { get; set; }
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
