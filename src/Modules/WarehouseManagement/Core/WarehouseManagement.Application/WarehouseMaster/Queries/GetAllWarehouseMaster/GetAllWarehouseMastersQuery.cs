using Contracts.Common;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster
{
    public class GetAllWarehouseMastersQuery  : IRequest <ApiResponseDTO<List<WarehouseMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}