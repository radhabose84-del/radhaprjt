using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation
{
    public class UpdateAssetLocationCommand :   IRequest<int>, IRequirePermission
    {

        public int AssetId { get; set; }
         public int UnitId { get; set; } 
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; } 
        public int CustodianId { get; set; }
        public int UserID { get; set; }     
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
