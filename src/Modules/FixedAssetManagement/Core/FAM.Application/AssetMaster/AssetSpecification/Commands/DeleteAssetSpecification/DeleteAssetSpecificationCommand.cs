using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification
{
    public class DeleteAssetSpecificationCommand :  IRequest<AssetSpecificationDTO>, IRequirePermission
    {
         public int Id { get; set; }    
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
