namespace Contracts.Dtos.Lookups.Production
{
    public sealed class PackTypeLookupDto
    {
        public int Id { get; set; }
        public string? PackTypeCode { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public int ConesPerBag { get; set; }
    }
}
