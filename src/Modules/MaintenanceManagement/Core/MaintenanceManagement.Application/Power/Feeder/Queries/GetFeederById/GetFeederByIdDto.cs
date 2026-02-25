namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById
{
    public class GetFeederByIdDto
    {
          public string FeederCode { get; set; } = string.Empty;
        public string FeederName { get; set; } = string.Empty;
        public int ParentFeederId { get; set; }
         public int FeederGroupId { get; set; }
        public int FeederTypeId { get; set; }
        public int  UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string? Description { get; set; }
        public byte MeterAvailable { get; set; }
        public int MeterTypeId { get; set; }
        public decimal MultiplicationFactor { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public decimal OpeningReading { get; set; }
        public byte HighPriority { get; set; }
        public byte IsActive { get; set; }
    }
}