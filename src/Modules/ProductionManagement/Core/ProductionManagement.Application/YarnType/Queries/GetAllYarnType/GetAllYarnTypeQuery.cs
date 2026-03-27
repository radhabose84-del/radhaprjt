using Contracts.Common;
using MediatR;
using ProductionManagement.Application.YarnType.Dto;

namespace ProductionManagement.Application.YarnType.Queries.GetAllYarnType
{
    public class GetAllYarnTypeQuery : IRequest<ApiResponseDTO<List<YarnTypeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
