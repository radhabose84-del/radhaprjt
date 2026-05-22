using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;
using Contracts.Common;

namespace FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster
{
    public class CreateSpecificationMasterCommand : IRequest<SpecificationMasterDTO>, IRequirePermission
    {
        public string? SpecificationName { get; set; }      
        public int? AssetGroupId { get; set; } 
        public byte ISDefault { get; set; }       

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
