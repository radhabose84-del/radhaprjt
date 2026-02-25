using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities.Power
{
    public class PowerConsumption : BaseEntity
    {
        public int FeederTypeId { get; set; }
        public MiscMaster? FeederTypePower { get; set; }
        public int FeederId { get; set; }
        public Feeder? FeederPower { get; set; }
        public int UnitId { get; set; }
        public decimal OpeningReading { get; set; }
        public decimal ClosingReading { get; set; }
        public decimal TotalUnits { get; set; }
    }
}