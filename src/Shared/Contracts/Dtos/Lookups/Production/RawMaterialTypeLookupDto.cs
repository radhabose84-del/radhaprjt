namespace Contracts.Dtos.Lookups.Production
{
    public sealed class RawMaterialTypeLookupDto
    {
        public int Id { get; set; }
        public string? RawMaterialTypeCode { get; set; }
        public string? RawMaterialTypeName { get; set; }
    }
}
