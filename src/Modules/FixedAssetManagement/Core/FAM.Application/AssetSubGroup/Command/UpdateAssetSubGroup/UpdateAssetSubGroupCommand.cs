using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup
{
    public class UpdateAssetSubGroupCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SubGroupName { get; set; }
        public int SortOrder { get; set; }
        public int GroupId { get; set; }
        public byte IsActive { get; set; }
        public decimal SubGroupPercentage { get; set; }  
        public byte AdditionalDepreciation { get; set; }      
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
