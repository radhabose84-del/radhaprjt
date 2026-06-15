using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup
{
    public class CreateAssetSubGroupCommand : IRequest<int>, IRequirePermission
    {
        public string? Code { get; set; }
        public string? SubGroupName { get; set; }
        public int GroupId { get; set; }
        public decimal SubGroupPercentage { get; set; }       
        public byte AdditionalDepreciation { get; set; }    
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
