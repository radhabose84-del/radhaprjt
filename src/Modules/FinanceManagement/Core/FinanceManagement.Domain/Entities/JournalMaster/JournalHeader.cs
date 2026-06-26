using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-01/04/05/07/09/10/12/13 — the journal voucher header. One row per journal. Carries
    // balance totals, lifecycle status, source/bypass metadata, reversal/copy links and status stamps.
    public class JournalHeader : BaseEntity
    {
        public int CompanyId { get; set; }              // cross-module FK — no DB constraint
        public int? UnitId { get; set; }                // cross-module FK — no DB constraint

        public int VoucherTypeId { get; set; }          // same-module FK -> VoucherTypeMaster
        public string? VoucherNo { get; set; }          // blank until posting; assigned by 03A
        public DateOnly VoucherDate { get; set; }
        public DateOnly? PostingDate { get; set; }

        public int FinancialYearId { get; set; }        // cross-module FK — no DB constraint
        public int? AccountingPeriodId { get; set; }    // same-module FK -> AccountingPeriod
        public string? Narration { get; set; }          // full-text indexed (US-14)

        public int StatusId { get; set; }               // same-module FK -> MiscMaster (JOURNAL_STATUS)
        public int SourceId { get; set; }               // same-module FK -> MiscMaster (JOURNAL_SOURCE)
        public string? TriggerDocType { get; set; }     // US-07: originating doc type (auto JV)
        public string? TriggerDocRef { get; set; }      // US-07: originating doc ref (auto JV)
        public bool AutoApproved { get; set; }

        public decimal TotalDr { get; set; }
        public decimal TotalCr { get; set; }

        public int? ReversalOfId { get; set; }          // self-FK -> original voucher (US-12)
        public bool IsReversal { get; set; }            // a reversal cannot be reversed
        public string? CopiedFromRef { get; set; }      // US-13: display only — no FK
        public int? ImportBatchId { get; set; }         // same-module FK -> JournalImportBatch (source = IMPORT)

        public DateTimeOffset? CleanupAlertedAt { get; set; }   // stale-draft alert stamp (replaces DraftCleanupLog)

        // Workflow stamps store the actor's NAME (not the user id) for direct display.
        public string? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public string? RejectedBy { get; set; }
        public DateTimeOffset? RejectedAt { get; set; }
        public string? RejectReason { get; set; }
        public string? PostedBy { get; set; }
        public DateTimeOffset? PostedAt { get; set; }

        // US-GL03-04 — backdating audit metadata.
        // IsBackdated is a PERSISTED COMPUTED column in SQL Server (set by the DB whenever
        // VoucherDate < CAST(PostedAt AS DATE)); never assigned in C#.
        public bool IsBackdated { get; private set; }
        // Required when posting to a SoftClosed prior period (US-GL03-04 AC#2); null otherwise.
        public string? BackdateReason { get; set; }
        public int? BackdateAcknowledgedBy { get; set; }
        public DateTimeOffset? BackdateAcknowledgedAt { get; set; }

        // Same-module FK navigation
        public VoucherTypeMaster? VoucherType { get; set; }
        public AccountingPeriod? AccountingPeriod { get; set; }
        public MiscMaster? JournalStatus { get; set; }  // named to avoid hiding BaseEntity.Status enum
        public MiscMaster? Source { get; set; }
        public JournalHeader? ReversalOf { get; set; }
        public JournalImportBatch? ImportBatch { get; set; }

        // Children
        public ICollection<JournalDetail>? Details { get; set; }
    }
}
