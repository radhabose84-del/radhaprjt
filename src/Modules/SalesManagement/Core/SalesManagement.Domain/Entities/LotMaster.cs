using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class LotMaster : BaseEntity
    {
        public string? LotCode { get; set; }
        public string? BatchNumber { get; set; }
        public int LotTypeId { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public DateOnly StartDate { get; set; }
        public int StatusId { get; set; }
        public string? ProductionOrderRef { get; set; }
        public decimal TotalProducedQty { get; set; }
        public decimal AvailableQty { get; set; }
        public DateOnly? RunOutDate { get; set; }
        public string? Remarks { get; set; }

        // Same-module navigation properties
        public MiscMaster? LotTypeMisc { get; set; }
        public MiscMaster? StatusMisc { get; set; }
    }
}
