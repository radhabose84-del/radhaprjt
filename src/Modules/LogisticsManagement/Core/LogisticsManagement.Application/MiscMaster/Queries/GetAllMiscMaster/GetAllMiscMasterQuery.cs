using Contracts.Common;
using MediatR;
using LogisticsManagement.Application.MiscMaster.Dto;

namespace LogisticsManagement.Application.MiscMaster.Queries.GetAllMiscMaster
{
    public class GetAllMiscMasterQuery : IRequest<ApiResponseDTO<List<MiscMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
