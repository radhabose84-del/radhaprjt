using Contracts.Common;
using MediatR;
using SalesManagement.Application.StoHeader.Dto;

namespace SalesManagement.Application.StoHeader.Queries.GetPendingStoHeader
{
    public class GetPendingStoHeaderQuery : IRequest<ApiResponseDTO<List<StoHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
