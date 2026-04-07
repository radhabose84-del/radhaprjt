namespace Contracts.Dtos.Lookups.Logistics;

public sealed class FreightMasterLookupDto
{
    public int Id { get; set; }
    public int FreightModeId { get; set; }
    public string? FreightModeName { get; set; }
    public int RateMethodId { get; set; }
    public string? RateMethodName { get; set; }
    public decimal Rate { get; set; }
    public int ModuleId { get; set; }
}
