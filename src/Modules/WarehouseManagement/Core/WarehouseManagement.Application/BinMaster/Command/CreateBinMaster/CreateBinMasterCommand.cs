using MediatR;
using Contracts.Common;

namespace WarehouseManagement.Application.BinMaster.Command.CreateBinMaster
{
    public class CreateBinMasterCommand    :   IRequest<int>, IRequirePermission
    {
     //   public string BinCode { get; set; } = string.Empty;
        public string BinName { get; set; } = string.Empty;

        public int WarehouseId { get; set; }

        public int? RackId { get; set; }

        public decimal BinCapacity { get; set; }
         
        public int CapacityUOMId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
