using MediatR;
using Contracts.Common;

namespace WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster
{
    public class UpdateBinMasterCommand : IRequest<int>, IRequirePermission
    {
         public int Id { get; set; }                 
        public string? BinName { get; set; }      
        public decimal BinCapacity { get; set; }    
        public int CapacityUOMId { get; set; }     
        public byte IsActive { get; set; }         
        public int? RackId { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
