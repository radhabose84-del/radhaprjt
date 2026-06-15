using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetGroup.Command.CreateAssetGroup
{
    public class CreateAssetGroupCommand : IRequest<int>, IRequirePermission
    {
        public string? Code { get; set; }
        public string? GroupName { get; set; }
        public decimal? GroupPercentage { get; set; }
        
       
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
