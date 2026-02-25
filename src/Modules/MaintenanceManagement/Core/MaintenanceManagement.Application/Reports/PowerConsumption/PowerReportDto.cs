namespace MaintenanceManagement.Application.Reports.PowerConsumption
{
    public class PowerReportDto
    {
    public int Id { get; set; }

    // Feeder Group
    public string? FeederGroupCode { get; set; }
    public string? FeederGroupName { get; set; }

    // Feeder Type (from MiscMaster)
    public string? FeederType { get; set; }

    // Feeder details
    public string? FeederCode { get; set; }
    public string? FeederName { get; set; }
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public decimal MultiplicationFactor { get; set; }
    public DateTimeOffset EffectiveDate { get; set; }

    // Readings
    public decimal OpeningReading { get; set; }
    public decimal ClosingReading { get; set; }
    public decimal TotalUnits { get; set; }

    // Audit Info
    public string? CreatedByName { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    }
}