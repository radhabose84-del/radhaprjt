namespace FinanceManagement.Application.TaxCode.Dto
{
    public class TaxCodeMasterLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxName { get; set; }
        public string? TaxType { get; set; }
        public string? TaxComponent { get; set; }
    }
}
