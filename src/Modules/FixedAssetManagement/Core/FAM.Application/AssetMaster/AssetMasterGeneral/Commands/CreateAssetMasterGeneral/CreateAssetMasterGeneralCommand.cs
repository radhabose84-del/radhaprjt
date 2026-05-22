using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral
{
    public class CreateAssetMasterGeneralCommand : IRequest<AssetMasterDto>, IRequirePermission
    {
       public AssetMasterDto? AssetMaster { get; set; }       
       public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
