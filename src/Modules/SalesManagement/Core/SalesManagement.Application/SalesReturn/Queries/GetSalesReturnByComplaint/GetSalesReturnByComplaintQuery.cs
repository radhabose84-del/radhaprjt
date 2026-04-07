using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.SalesReturn.Queries.GetSalesReturnByComplaint
{
    public class GetSalesReturnByComplaintQuery : IRequest<ApiResponseDTO<List<SalesReturnHeaderDto>>>
    {
        public int ComplaintHeaderId { get; set; }
    }
}
