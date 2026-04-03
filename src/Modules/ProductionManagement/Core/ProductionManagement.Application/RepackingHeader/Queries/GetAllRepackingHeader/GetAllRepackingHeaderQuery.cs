using Contracts.Common;
using MediatR;
using ProductionManagement.Application.RepackingHeader.Dto;

namespace ProductionManagement.Application.RepackingHeader.Queries.GetAllRepackingHeader
{
    public class GetAllRepackingHeaderQuery : IRequest<ApiResponseDTO<List<RepackingHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? TypeId { get; set; }
    }
}
