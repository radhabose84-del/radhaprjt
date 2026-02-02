using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster
{
    public class UpdatePaymentTermMasterCommand  : IRequest<bool>
    {
        public int Id { get; set; }

        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int BaselineTypeId { get; set; }
        public int CreditDays { get; set; }
        public decimal? AdvancePercent { get; set; }
        public decimal? DiscountPercent { get; set; }
        public int? DiscountDays { get; set; }
        public int? GraceDays { get; set; }
        public bool ApplicableForPortal { get; set; }
         public bool IsActive { get; set; } 
       


        // Replace strategy: full list of installments to persist
        public List<PaymentTermInstallmentDto>? Installments { get; set; }
    }
}