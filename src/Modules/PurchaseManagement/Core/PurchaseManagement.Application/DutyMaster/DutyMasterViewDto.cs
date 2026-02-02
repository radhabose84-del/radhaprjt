namespace PurchaseManagement.Application.DutyMaster;
public sealed class DutyMasterViewDto : DutyMasterDto
{    
    public HsnInfo? Hsn { get; set; }    
    public string? DutyCategoryName { get; set; }
    public string? CountryOfOriginApplicabilityName { get; set; } 
}

public sealed class HsnInfo
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public string? Gst { get; set; }
    public string? IGst { get; set; }    
}
