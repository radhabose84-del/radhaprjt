namespace FinanceManagement.Application.CoaRead.Dto
{
    // US-GL02-16 — slim, downstream-facing read projection of a GL account (no audit/governance noise).
    public class CoaAccountReadDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        public int AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }
        public int AccountGroupId { get; set; }
        public string? AccountGroupCode { get; set; }
        public string? AccountGroupName { get; set; }

        public int CurrencyTypeId { get; set; }
        public string? CurrencyTypeCode { get; set; }
        public string? NormalBalanceCode { get; set; }

        public bool IsCostCentreMandatory { get; set; }
        public bool IsActive { get; set; }   // AC5 — status returned with each row
    }
}
