namespace FinanceManagement.Application.VoucherType.Dto
{
    public class VoucherTypeMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? VoucherTypeCode { get; set; }
        public string? VoucherTypeName { get; set; }
        public int NumberPadding { get; set; }
        public bool IsSystem { get; set; }

        // Active fiscal-year next number (e.g. "JV/2026-27/0428") — populated in GetAll
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }
        public int LastUsedNumber { get; set; }
        public string? NextNumber { get; set; }

        public List<VoucherTypeAccountTypeDto> AllowedAccountTypes { get; set; } = new();

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
