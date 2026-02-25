using MediatR;

namespace WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster
{
    public class DeleteRackMasterCommand : IRequest<bool>
    {
        public int  Id { get; set; }
    }
}