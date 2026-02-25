using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES
{
    public class GetSESListQuery  : IRequest<ApiResponseDTO<List<GetServiceEntrySheetListDto>>>
    {
         public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}