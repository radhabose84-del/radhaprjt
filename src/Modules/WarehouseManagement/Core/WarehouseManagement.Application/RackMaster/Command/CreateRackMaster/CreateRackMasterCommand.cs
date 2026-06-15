using MediatR;
using Contracts.Common;

namespace WarehouseManagement.Application.RackMaster.Command.CreateRackMaster
{
    public class CreateRackMasterCommand :   IRequest<int>, IRequirePermission
    {
        
        public int WarehouseId { get; set; }         
       /// public string RackCode { get; set; } = default!;    
        public string? RackName { get; set; }
        public int? FloorId { get; set; }        
        public int? AisleId { get; set; }        
        public int? RackLevelId { get; set; }        
        public decimal? MaxCapacity { get; set; }
        public int? CapacityUOMId { get; set; }        
        public decimal? RackWidth { get; set; }
        public decimal? RackHeight { get; set; }
        public int? DimensionUOMId { get; set; }
        
        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
