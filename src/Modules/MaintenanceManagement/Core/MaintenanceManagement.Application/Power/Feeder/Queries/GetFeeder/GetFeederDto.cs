using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder
{
    public class GetFeederDto
    {
        public int Id { get; set;}
        public string FeederCode { get; set; } = string.Empty;
        public string FeederName { get; set; } = string.Empty;
        public int? ParentFeederId { get; set; }
        public int FeederGroupId { get; set; }
        public int FeederTypeId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public byte MeterAvailable { get; set; }
        public int? MeterTypeId { get; set; }
        public string? MeterType { get; set; }
        public int DepartmentId { get; set; }
        public string ? DepartmentName { get; set; }
        public string? Description { get; set; }
        public decimal MultiplicationFactor { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public decimal OpeningReading { get; set; }
        public byte HighPriority { get; set; }

        public decimal? Target { get; set; }
       
        public Status IsActive { get; set; }


        
    }
}