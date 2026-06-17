namespace FinanceManagement.Application.TaxCode.Dto
{
    // Tax Code Registry summary — one row per tax code with its current rate and the count of
    // GL accounts currently linked to it (the "GL MAPPING" column: e.g. "1 account" / "No GL mapping").
    public class TaxCodeGlMappingSummaryDto
    {
        public int TaxCodeId { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxName { get; set; }

        public int TaxTypeId { get; set; }
        public string? TaxType { get; set; }                 // MiscMaster code (GST_OUT/GST_IN/TDS/...)

        public decimal? CurrentRatePercent { get; set; }

        public int GlAccountCount { get; set; }              // distinct active GL accounts linked to this code

        public bool IsActive { get; set; }                   // tax code activation status
    }
}
