using Contracts.Common;
using MediatR;
using ProductionManagement.Application.ProcessMaster.Dto;

namespace ProductionManagement.Application.ProcessMaster.Queries.GetAllProcessMaster
{
    public class GetAllProcessMasterQuery : IRequest<ApiResponseDTO<List<ProcessMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
