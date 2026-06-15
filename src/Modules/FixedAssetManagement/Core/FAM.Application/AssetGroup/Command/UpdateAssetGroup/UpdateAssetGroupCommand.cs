using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetGroup.Command.UpdateAssetGroup
{
    public class UpdateAssetGroupCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public int SortOrder { get; set; }
        public byte IsActive { get; set; }
        public decimal? GroupPercentage { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
