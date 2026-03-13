using Contracts.Common;
using FinanceManagement.Application.EWaybillHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Queries.GetAllEWaybillHeader
{
    public class GetAllEWaybillHeaderQuery : IRequest<ApiResponseDTO<List<EWaybillHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
