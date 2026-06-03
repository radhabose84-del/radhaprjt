using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class TransactionTypeMaster : BaseEntity
    {
        public int UnitId { get; set; }
        public int ModuleId { get; set; }
        public int MenuId { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }

        // Marks the transaction type as gate-related. Defaults to false; flipped to true
        // only for entries that should surface in Gate flows.
        public bool IsGate { get; set; }

        // Inverse navigation for DocumentSequence (same-module FK)
        public ICollection<DocumentSequence>? DocumentSequences { get; set; }
    }
}
