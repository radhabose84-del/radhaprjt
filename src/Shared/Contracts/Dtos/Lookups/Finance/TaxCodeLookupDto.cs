namespace Contracts.Dtos.Lookups.Finance
{
    public sealed class TaxCodeLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxName { get; set; }
        public string? TaxType { get; set; }
        public string? TaxComponent { get; set; }
        public string? Direction { get; set; }
        public bool IsSystemOnlyPosting { get; set; }

        // Rate effective on the requested date (null when resolving a plain list).
        public decimal? RatePercent { get; set; }
        public string? StatutorySection { get; set; }
    }
}
