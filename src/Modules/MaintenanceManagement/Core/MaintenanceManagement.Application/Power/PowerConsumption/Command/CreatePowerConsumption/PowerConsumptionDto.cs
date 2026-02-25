using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption
{
    public class PowerConsumptionDto
    {
        public int Id { get; set; }
        public int FeederTypeId { get; set; }
        public int FeederId { get; set; }
        public int UnitId { get; set; }
        public decimal OpeningReading { get; set; }
        public decimal ClosingReading { get; set; }
        public decimal TotalUnits { get; set; }
        public Status IsActive { get; set; }
    }
}