using Contracts.Common;
using MediatR;
using ProductionManagement.Application.PackType.Dto;

namespace ProductionManagement.Application.PackType.Queries.GetAllPackType
{
    public class GetAllPackTypeQuery : IRequest<ApiResponseDTO<List<PackTypeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
