using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.SalesReturn.Queries.GetComplaintReturnData
{
    public class GetComplaintReturnDataQuery : IRequest<ApiResponseDTO<ComplaintReturnDataDto>>
    {
        public int ComplaintHeaderId { get; set; }
    }
}
