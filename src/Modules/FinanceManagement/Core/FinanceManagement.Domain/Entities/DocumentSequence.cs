using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class DocumentSequence : BaseEntity
    {
        public int TransactionTypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int DocNo { get; set; }

        // Same-module navigation property
        public TransactionTypeMaster? TransactionTypeMaster { get; set; }
    }
}
