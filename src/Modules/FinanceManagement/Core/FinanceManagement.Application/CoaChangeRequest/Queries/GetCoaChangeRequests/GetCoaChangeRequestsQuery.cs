using Contracts.Common;
using FinanceManagement.Application.CoaChangeRequest.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaChangeRequests
{
    // US-GL02-08B — list change requests for the session company, optionally filtered by status.
    public class GetCoaChangeRequestsQuery : IRequest<ApiResponseDTO<List<CoaChangeRequestDto>>>
    {
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
