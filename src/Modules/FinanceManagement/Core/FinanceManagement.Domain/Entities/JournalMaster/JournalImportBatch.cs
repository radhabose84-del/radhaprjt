using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-17 — header record for an Excel import run. Valid rows commit as Drafts tagged
    // source = IMPORT; nothing commits if any row errors (no partial commit).
    public class JournalImportBatch : BaseEntity
    {
        public string? FileName { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int ErrorRows { get; set; }

        public int StatusId { get; set; }               // same-module FK -> MiscMaster (IMPORT_BATCH_STATUS)
        public int SourceId { get; set; }               // same-module FK -> MiscMaster (JOURNAL_SOURCE = IMPORT)
        public int ImportedBy { get; set; }             // cross-module — no DB constraint

        // Same-module FK navigation
        public MiscMaster? BatchStatus { get; set; }
        public MiscMaster? Source { get; set; }

        // Children
        public ICollection<JournalImportError>? Errors { get; set; }
    }
}
