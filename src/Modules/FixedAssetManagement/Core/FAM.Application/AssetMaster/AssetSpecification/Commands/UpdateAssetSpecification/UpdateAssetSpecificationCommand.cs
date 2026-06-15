using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification
{
    public class UpdateAssetSpecificationCommand : IRequest<string>, IRequirePermission
    {
        public int AssetId { get; set; }
        public List<UpdateSpecificationItem>? Specifications { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }

    public class UpdateSpecificationItem
    {
        public int SpecificationId { get; set; }
        public string? SpecificationValue { get; set; }
        public byte IsActive  { get; set; }
    }
}
