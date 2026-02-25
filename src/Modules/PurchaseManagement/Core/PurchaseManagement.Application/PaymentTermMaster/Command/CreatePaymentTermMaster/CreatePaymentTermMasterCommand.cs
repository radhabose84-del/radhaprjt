using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster
{
    public class CreatePaymentTermMasterCommand : IRequest<int>
    {
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int BaselineTypeId { get; set; }
        public int CreditDays { get; set; }
        public decimal? AdvancePercent { get; set; }        
        public decimal? DiscountPercent { get; set; }
        public int? DiscountDays { get; set; }
        public int? GraceDays { get; set; }
        public bool ApplicableForPortal { get; set; }
        public List<PaymentTermInstallmentDto>? Installments { get; set; }
    }
}