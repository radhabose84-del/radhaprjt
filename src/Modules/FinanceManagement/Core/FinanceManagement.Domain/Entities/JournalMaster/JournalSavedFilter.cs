namespace FinanceManagement.Domain.Entities
{
    // US-GL01-14 — per-user named filter combinations for the journal search screen.
    // Lean preference table (NOT a BaseEntity).
    public class JournalSavedFilter
    {
        public int Id { get; set; }

        public int UserId { get; set; }                 // cross-module — no DB constraint
        public string? Name { get; set; }
        public string? CriteriaJson { get; set; }       // serialized filter criteria
    }
}
