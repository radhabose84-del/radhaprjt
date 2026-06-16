namespace FinanceManagement.Application.TaxCode.Dto
{
    public class TaxAccountLinkageDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int TaxCodeId { get; set; }
        public string? TaxCode { get; set; }              // same-module join
        public string? TaxName { get; set; }

        public int GlAccountId { get; set; }
        public string? AccountCode { get; set; }          // same-module join (GlAccountMaster)
        public string? AccountName { get; set; }

        public bool IsActivated { get; set; }
        public string? ApprovalStatus { get; set; }

        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

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
