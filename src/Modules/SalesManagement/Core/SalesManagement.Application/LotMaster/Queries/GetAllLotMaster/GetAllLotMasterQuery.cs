using Contracts.Common;
using MediatR;
using SalesManagement.Application.LotMaster.Dto;

namespace SalesManagement.Application.LotMaster.Queries.GetAllLotMaster
{
    public class GetAllLotMasterQuery : IRequest<ApiResponseDTO<List<LotMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
