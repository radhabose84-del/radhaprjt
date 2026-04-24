using Contracts.Common;
using MediatR;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.Application.LotMaster.Queries.GetAllLotMaster
{
    public class GetAllLotMasterQuery : IRequest<ApiResponseDTO<List<LotMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? ItemId { get; set; }
    }
}
