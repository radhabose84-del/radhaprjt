using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class TnCTemplateApplicability : BaseEntity
    {
        
        public int TnCTemplateMasterId { get; set; }
        public TnCTemplateMaster TnCTemplate { get; set; } = null!;

        // Transaction type (cross-module FK to Finance.TransactionTypeMaster — no DB constraint,
        // populated via ITransactionTypeLookup)
        public int TransactionTypeId { get; set; }
    }
}