using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster
{
    public class DeleteWarehouseMasterCommand  : IRequest<bool>
    {
        public int  Id { get; set; }
    }
}