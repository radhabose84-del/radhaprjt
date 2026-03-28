using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Repacking.Dto;

namespace ProductionManagement.Application.Repacking.Queries.GetAllRepacking
{
    public class GetAllRepackingQuery : IRequest<ApiResponseDTO<List<RepackingHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
