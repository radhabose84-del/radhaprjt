using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class PaymentTermInstallment : BaseEntity
    {

        public int PaymentTermId { get; set; }
        public PaymentTermMaster PaymentTerm { get; set; } = default!;
        public int SeqNo { get; set; }        // 1..N
        public decimal Percent { get; set; }  // 0..100 (rows must sum to 100)
        public int DueDays { get; set; } 
        
    }
}