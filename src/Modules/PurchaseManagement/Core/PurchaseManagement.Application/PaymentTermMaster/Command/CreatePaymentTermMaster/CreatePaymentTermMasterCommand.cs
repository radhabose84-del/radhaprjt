using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster
{
    public class CreatePaymentTermMasterCommand : IRequest<int>, IRequirePermission
    {
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int BaselineTypeId { get; set; }
        public int CreditDays { get; set; }
        public decimal? AdvancePercent { get; set; }        
        public decimal? DiscountPercent { get; set; }
        public int? DiscountDays { get; set; }
        public int? GraceDays { get; set; }
        public decimal AdditionalValue { get; set; }
        public bool ApplicableForPortal { get; set; }
        public List<PaymentTermInstallmentDto>? Installments { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
