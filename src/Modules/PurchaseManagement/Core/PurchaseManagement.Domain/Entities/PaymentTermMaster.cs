using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class PaymentTermMaster  : BaseEntity
    {
         public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;       
        public int BaselineTypeId { get; set; }
        public MiscMaster BaselineType { get; set; } = default!;
        public int CreditDays { get; set; }
        public decimal? AdvancePercent { get; set; }
        public decimal BalancePercent { get; private set; }
        public decimal? DiscountPercent { get; set; }   
        public int? DiscountDays { get; set; }
        public int? GraceDays { get; set; }
        public bool ApplicableForPortal { get; set; }       
        public ICollection<PaymentTermInstallment> Installments { get; set; } = new List<PaymentTermInstallment>();
    }
}