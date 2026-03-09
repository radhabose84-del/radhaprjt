using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DocumentSequence : BaseEntity
    {
        public int TypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int DocNo { get; set; }

        // Same-module navigation property
        public TransactionTypeMaster? TransactionTypeMaster { get; set; }
    }
}
