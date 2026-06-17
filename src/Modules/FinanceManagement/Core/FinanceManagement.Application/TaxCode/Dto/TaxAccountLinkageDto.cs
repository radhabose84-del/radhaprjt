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

        public int? ControlAccountId { get; set; }
        public string? ControlAccountName { get; set; }     // same-module join (MiscMaster)

        public int StatusId { get; set; }
        public string? Status { get; set; }                 // MiscMaster code (join): PENDING/APPROVED/REJECTED

        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        public string? ChangeReason { get; set; }           // justification for a PENDING change request

        public bool IsActive { get; set; }

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
