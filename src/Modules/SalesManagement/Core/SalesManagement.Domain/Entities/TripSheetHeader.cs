using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class TripSheetHeader : BaseEntity
    {
        public string? TripSheetNo { get; set; }
        public DateOnly TripDate { get; set; }
        public string? VehicleNo { get; set; }
        public int UnitId { get; set; }                    // Cross-module FK → UserManagement.Unit
        public string? Remarks { get; set; }

        // Child collection
        public ICollection<TripSheetDetail>? TripSheetDetails { get; set; }
    }
}
