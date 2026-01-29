using Core.Application.Common.Mappings;

namespace Core.Application.Units.Queries.GetUnits
{
    public class UnitAddressDto
    {
    public int CountryId { get; set; }
    public int StateId { get; set; }
    public int CityId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public int PinCode { get; set; }
    public string? ContactNumber { get; set; }
    public string? AlternateNumber { get; set; }
    }
}