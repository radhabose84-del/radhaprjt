using MediatR;

namespace WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster
{
    public class DeleteBinMasterCommand : IRequest<bool>
    {
        public int  Id { get; set; }
        
    }
}