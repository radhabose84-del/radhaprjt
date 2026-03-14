using Contracts.Common;
using MediatR;
using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.Application.CountMaster.Queries.GetAllCountMaster
{
    public class GetAllCountMasterQuery : IRequest<ApiResponseDTO<List<CountMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
