using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification
{
    public class CreateAssetSpecificationCommand : IRequest<string>, IRequirePermission
    {
        public int AssetId { get; set; }
        public List<SpecificationItem>? Specifications { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }

    public class SpecificationItem
    {
        public int SpecificationId { get; set; }
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }        
    }
}
