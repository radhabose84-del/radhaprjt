using MediatR;

namespace InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscMasterCommand  : IRequest<bool>
    {
          public int Id { get; set; }
                
    }
}