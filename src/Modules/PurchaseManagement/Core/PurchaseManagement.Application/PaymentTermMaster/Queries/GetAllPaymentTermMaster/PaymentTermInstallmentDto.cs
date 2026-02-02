    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster
    {
    public class PaymentTermInstallmentDto
    {
        // public int Id { get; set; }
        public int PaymentTermId { get; set; }
        public int SeqNo { get; set; }
        public decimal Percent { get; set; }
        public int DueDays { get; set; } 
            

        }
    }