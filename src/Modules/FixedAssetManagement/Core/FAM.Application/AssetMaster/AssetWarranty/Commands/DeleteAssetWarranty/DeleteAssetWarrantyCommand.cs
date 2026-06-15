using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty
{
    public class DeleteAssetWarrantyCommand :  IRequest<AssetWarrantyDTO>, IRequirePermission
    {
         public int Id { get; set; }    
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
