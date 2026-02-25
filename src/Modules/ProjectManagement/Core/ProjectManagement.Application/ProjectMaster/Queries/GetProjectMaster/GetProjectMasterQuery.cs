using MediatR;
using Contracts.Common;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster
{
    public class GetProjectMasterQuery : IRequest<ApiResponseDTO<List<GetProjectMasterDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
    }
}