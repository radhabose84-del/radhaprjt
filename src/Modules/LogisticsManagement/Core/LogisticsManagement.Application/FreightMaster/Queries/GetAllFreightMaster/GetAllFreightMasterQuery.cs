using Contracts.Common;
using MediatR;
using LogisticsManagement.Application.FreightMaster.Dto;

namespace LogisticsManagement.Application.FreightMaster.Queries.GetAllFreightMaster
{
    public class GetAllFreightMasterQuery : IRequest<ApiResponseDTO<List<FreightMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
