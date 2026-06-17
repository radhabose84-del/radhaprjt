namespace FinanceManagement.Application.CurrencyForexConfig.Dto
{
    public class CurrencyForexConfigLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CurrencyTypeCode { get; set; }
        public string? CurrencyTypeName { get; set; }
    }
}
