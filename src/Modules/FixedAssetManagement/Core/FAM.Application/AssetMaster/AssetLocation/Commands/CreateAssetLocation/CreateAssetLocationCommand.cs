using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetLocation.Commands.CreateAssetLocation
{
    public class CreateAssetLocationCommand : IRequest<AssetLocationDto>, IRequirePermission
    {
     
        public int AssetId { get; set; }
        public int UnitId { get; set; } 
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; } 
        public int CustodianId { get; set; }
        public int UserID { get; set; }
        

        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
