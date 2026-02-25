using Contracts.Common;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.GetWarehouseMasterById
{
    public class GetWarehouseMasterByIdQuery : IRequest<ApiResponseDTO<WarehouseMasterDto>>
    {
        public int Id { get; set; }
        
    }
}