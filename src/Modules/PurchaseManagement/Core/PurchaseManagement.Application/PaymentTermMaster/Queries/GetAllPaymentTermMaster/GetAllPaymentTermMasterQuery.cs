using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster
{
    public class GetAllPaymentTermMasterQuery    : IRequest <ApiResponseDTO<List<PaymentTermMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }    
        
    }
}