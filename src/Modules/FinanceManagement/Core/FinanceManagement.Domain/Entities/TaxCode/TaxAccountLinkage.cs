using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-05B — links a (component) tax code to a GL control account. Effective-dated
    // rows + Finance.ActivityLog (IActivityTracked) give the change history (Change Audit tab).
    // No hard/soft delete and no IsDeleted column — "remove" = IsActive = Inactive.
    // Updates insert a NEW row; the superseded row is set IsActive = Inactive (+ EffectiveTo).
    // Initial create is auto-APPROVED; modifying TaxCodeId/ControlAccountId goes to PENDING approval
    // (dual approval FC + Tax Lead) owned by the BackgroundService Workflow module.
    public class TaxAccountLinkage : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }
        public int? TaxCodeId { get; set; }             // same-module FK -> TaxCodeMaster (nullable — a linkage may carry no tax code)
        public int GlAccountId { get; set; }            // same-module FK -> GlAccountMaster
        public int? ControlAccountId { get; set; }      // same-module FK -> MiscMaster (NEW control account type)

        // On a change-request row, points to the prior active linkage being superseded (null on initial
        // create). Old tax code / control account are read from that row for the Change-Audit "Old" columns.
        public int? OldTaxLinkageId { get; set; }        // self-FK -> TaxAccountLinkage (the superseded row)

        public int StatusId { get; set; }               // FK -> MiscMaster (ApprovalStatus): PENDING/APPROVED/REJECTED

        public DateOnly EffectiveFrom { get; set; }     // approval date; applies on/after (AC4-B)
        public DateOnly? EffectiveTo { get; set; }      // NULL = current; set when superseded -> history

        // Free-text justification for a change request (PENDING rows). Interim home until the
        // BackgroundService Workflow module's ApprovalRequest owns it. ActivityLog still tracks the field diffs.
        public string? ChangeReason { get; set; }

        // Same-module FK navigations
        public TaxCodeMaster? TaxCode { get; set; }
        public GlAccountMaster? GlAccount { get; set; }
        public MiscMaster? ControlAccount { get; set; }
        public MiscMaster? StatusMaster { get; set; }    // FK nav -> MiscMaster (ApprovalStatus); named to avoid hiding BaseEntity.Status
        public TaxAccountLinkage? OldTaxLinkage { get; set; }   // self-FK nav -> superseded linkage row
    }
}
