namespace PurchaseManagement.Application.Port.Dto;

public sealed class PortMasterDto
{
    public int Id { get; set; }
    public string PortCode { get; set; } = default!;
    public string PortName { get; set; } = default!;
    public int CountryId { get; set; }    
    public int PortTypeId { get; set; }
    public int IsActive { get; set; }    
    public string? PortType { get; set; }
    public string? Country { get; set; }
}

