using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Queries.GetRackMasterById
{
    public class GetRackMasterByIdQuery : IRequest<RackMasterDto>
    {
        public int Id { get; set; }
        
    }
}