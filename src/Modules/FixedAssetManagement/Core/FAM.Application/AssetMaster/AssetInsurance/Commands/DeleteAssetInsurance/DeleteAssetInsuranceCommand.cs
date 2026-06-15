using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance
{
    public class DeleteAssetInsuranceCommand :  IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }  
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
