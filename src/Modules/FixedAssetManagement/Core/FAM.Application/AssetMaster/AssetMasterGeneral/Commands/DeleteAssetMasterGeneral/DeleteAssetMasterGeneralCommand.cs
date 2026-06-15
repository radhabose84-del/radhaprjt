
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral
{
    public class DeleteAssetMasterGeneralCommand :  IRequest<AssetMasterGeneralDTO>, IRequirePermission
    {
        public int Id { get; set; }     
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
