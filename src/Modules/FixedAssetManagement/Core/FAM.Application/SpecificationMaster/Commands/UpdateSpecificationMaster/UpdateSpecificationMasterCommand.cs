using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;
using Contracts.Common;

namespace FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster
{
    public class UpdateSpecificationMasterCommand  : IRequest<SpecificationMasterDTO>, IRequirePermission
    {
        public int Id { get; set; }       
        public string? SpecificationName { get; set; }
        public int? AssetGroupId { get; set; }
        public byte IsDefault { get; set; }               
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
