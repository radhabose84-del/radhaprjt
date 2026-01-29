public class AssetSpecificationJsonDto
{
    public int AssetId { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }    
    public List<SpecificationDto> Specifications { get; set; } = new();
}

public class SpecificationDto  // Make sure the name matches here
{
    public int SpecificationId { get; set; }
    public string? SpecificationName { get; set; }
    public string? SpecificationValue { get; set; }
    public bool? ISDefault { get; set; }   
}
