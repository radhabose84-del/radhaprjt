namespace SalesManagement.Application.TripSheet.Dto
{
    public class TripSheetHeaderDto
    {
        public int Id { get; set; }
        public string? TripSheetNo { get; set; }
        public DateOnly TripDate { get; set; }
        public string? VehicleNo { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }              // IUnitLookup (cross-module)
        public string? Remarks { get; set; }
        public int TotalDispatches { get; set; }           // Computed
        public int TotalInvoiced { get; set; }             // Computed
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

        // Detail entries
        public List<TripSheetDetailDto>? Details { get; set; }
    }
}
