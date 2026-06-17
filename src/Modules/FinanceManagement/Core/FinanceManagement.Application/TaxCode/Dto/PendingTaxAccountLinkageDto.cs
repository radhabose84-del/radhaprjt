namespace FinanceManagement.Application.TaxCode.Dto
{
    // Shape for the Tax-Account Linkage "Change Audit / Pending" grid.
    public class PendingTaxAccountLinkageDto
    {
        public int Id { get; set; }                          // S.No
        public int CompanyId { get; set; }

        public int GlAccountId { get; set; }                 // Account
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        public int? OldTaxLinkageId { get; set; }            // FK to the superseded TaxAccountLinkage row
        public int? OldTaxCodeId { get; set; }               // Old Tax Code (read from the superseded linkage row)
        public string? OldTaxCode { get; set; }

        public int NewTaxCodeId { get; set; }                // New Tax Code (the requested change)
        public string? NewTaxCode { get; set; }
        public string? NewTaxName { get; set; }

        public int? OldControlAccountId { get; set; }
        public string? OldControlAccountName { get; set; }

        public int? ControlAccountId { get; set; }           // New control account
        public string? ControlAccountName { get; set; }

        public string? ChangeReason { get; set; }            // Reason

        public string? Approver1Name { get; set; }           // Approver 1 (FC)  — populated by the Workflow module on approval; null while pending
        public string? Approver2Name { get; set; }           // Approver 2 (Tax) — populated by the Workflow module on approval; null while pending

        public DateOnly EffectiveFrom { get; set; }          // Effective From
        public int StatusId { get; set; }
        public string? Status { get; set; }                  // Status (Pending/Approved/Rejected)

        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}
