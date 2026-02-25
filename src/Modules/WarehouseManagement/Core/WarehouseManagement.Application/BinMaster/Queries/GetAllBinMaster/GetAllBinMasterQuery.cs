using Contracts.Common;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster
{
    public class GetAllBinMasterQuery    : IRequest <ApiResponseDTO<List<BinMasterDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}