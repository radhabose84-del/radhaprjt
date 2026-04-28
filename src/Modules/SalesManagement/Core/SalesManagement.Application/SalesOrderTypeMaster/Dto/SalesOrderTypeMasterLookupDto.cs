namespace SalesManagement.Application.SalesOrderTypeMaster.Dto
{
    public sealed class SalesOrderTypeMasterLookupDto
    {
        public int Id { get; set; }
        public int SoTypeId { get; set; }
        public string? SoTypeCode { get; set; }
        public int TaxTypeId { get; set; }
        public string? TaxTypeShortName { get; set; }
        public string? TypeName { get; set; }
    }
}
