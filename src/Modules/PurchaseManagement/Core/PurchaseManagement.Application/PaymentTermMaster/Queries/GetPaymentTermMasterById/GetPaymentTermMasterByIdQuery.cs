using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermMasterById
{
    public class GetPaymentTermMasterByIdQuery  : IRequest<PaymentTermMasterDto>
    {
       public int Id { get; set; } 
        
    }
}