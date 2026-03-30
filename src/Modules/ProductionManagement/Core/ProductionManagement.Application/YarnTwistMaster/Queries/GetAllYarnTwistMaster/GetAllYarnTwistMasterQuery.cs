using Contracts.Common;
using MediatR;
using ProductionManagement.Application.YarnTwistMaster.Dto;

namespace ProductionManagement.Application.YarnTwistMaster.Queries.GetAllYarnTwistMaster
{
    public class GetAllYarnTwistMasterQuery : IRequest<ApiResponseDTO<List<YarnTwistMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
