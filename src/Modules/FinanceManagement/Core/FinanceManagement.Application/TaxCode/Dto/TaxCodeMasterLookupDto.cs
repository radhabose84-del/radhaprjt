namespace FinanceManagement.Application.TaxCode.Dto
{
    public class TaxCodeMasterLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxName { get; set; }
        public int TaxTypeId { get; set; }
        public string? TaxType { get; set; }              // MiscMaster code (join)
        public int? TaxComponentId { get; set; }
        public string? TaxComponent { get; set; }         // MiscMaster code (join)
        public decimal? CurrentRatePercent { get; set; }  // open rate version (by-name includes rate)
    }
}
