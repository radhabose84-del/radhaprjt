namespace Contracts.Dtos.Lookups.Production
{
    public sealed class YarnTypeLookupDto
    {
        public int Id { get; set; }
        public string? YarnTypeCode { get; set; }
        public string? YarnTypeName { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public int? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencyName { get; set; }
    }
}
