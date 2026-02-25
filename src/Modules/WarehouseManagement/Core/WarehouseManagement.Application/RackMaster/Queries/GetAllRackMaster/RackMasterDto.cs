namespace WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster
{
    public class RackMasterDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string RackCode { get; set; } = default!;
        public string? RackName { get; set; }
        public int? FloorId { get; set; }
        public string? FloorName { get; set; }
        public int? AisleId { get; set; }
        public string? AisleName { get; set; }
        public int? RackLevelId { get; set; }
        public string? RackLevelName { get; set; }
        public decimal? MaxCapacity { get; set; }
        public int? CapacityUOMId { get; set; }
        public string? CapacityUOMName { get; set; }
        public decimal? RackWidth { get; set; }
        public decimal? RackHeight { get; set; }
        public int? DimensionUOMId { get; set; }
        public string? DimensionUOMName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }         
      
        
    }
}