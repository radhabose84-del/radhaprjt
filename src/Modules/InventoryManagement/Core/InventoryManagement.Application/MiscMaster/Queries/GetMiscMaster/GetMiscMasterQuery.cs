using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster
{
    public class GetMiscMasterQuery  :IRequest<ApiResponseDTO<List<GetMiscMasterDto>>>
    {
          
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}