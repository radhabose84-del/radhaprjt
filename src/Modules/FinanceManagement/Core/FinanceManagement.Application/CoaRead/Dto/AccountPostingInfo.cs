namespace FinanceManagement.Application.CoaRead.Dto
{
    // US-GL02-16 — minimal fields the validate-for-posting logic needs (and the deactivation hook).
    public class AccountPostingInfo
    {
        public int Id { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public bool IsActive { get; set; }
        public int CurrencyTypeId { get; set; }
        public string? CurrencyTypeCode { get; set; }
        public bool IsCostCentreMandatory { get; set; }
    }
}
