using Contracts.Common;
using MediatR;
using ProductionManagement.Application.RepackingMaster.Dto;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetAllRepackingMaster
{
    public class GetAllRepackingMasterQuery : IRequest<ApiResponseDTO<List<RepackingMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
