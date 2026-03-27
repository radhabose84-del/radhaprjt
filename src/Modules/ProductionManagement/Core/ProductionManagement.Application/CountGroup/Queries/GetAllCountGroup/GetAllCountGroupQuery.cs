using Contracts.Common;
using MediatR;
using ProductionManagement.Application.CountGroup.Dto;

namespace ProductionManagement.Application.CountGroup.Queries.GetAllCountGroup
{
    public class GetAllCountGroupQuery : IRequest<ApiResponseDTO<List<CountGroupDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
