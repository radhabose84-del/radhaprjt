using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-05B — links a (component) tax code to a GL control account. Effective-dated
    // rows give the change history (Change Audit tab). Dual approval (FC + Tax Lead) is owned
    // by the BackgroundService Workflow module; this row only mirrors the outcome in ApprovalStatus.
    public class TaxAccountLinkage : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }
        public int TaxCodeId { get; set; }              // same-module FK -> TaxCodeMaster
        public int GlAccountId { get; set; }            // same-module FK -> GlAccountMaster

        public bool IsActivated { get; set; }           // AC2-B: only when GL present + workflow-approved
        public string? ApprovalStatus { get; set; }     // PENDING / APPROVED / REJECTED (set by Workflow callback)

        public DateOnly EffectiveFrom { get; set; }     // approval date; applies on/after (AC4-B)
        public DateOnly? EffectiveTo { get; set; }      // NULL = current; set when superseded -> history

        // Same-module FK navigations
        public TaxCodeMaster? TaxCode { get; set; }
        public GlAccountMaster? GlAccount { get; set; }
    }
}
