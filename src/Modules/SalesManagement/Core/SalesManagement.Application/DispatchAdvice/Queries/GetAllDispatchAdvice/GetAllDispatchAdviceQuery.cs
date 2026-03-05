using Contracts.Common;
using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetAllDispatchAdvice
{
    public class GetAllDispatchAdviceQuery : IRequest<ApiResponseDTO<List<DispatchAdviceHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
