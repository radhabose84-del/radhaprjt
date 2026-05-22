using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;
using Contracts.Common;

namespace FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster
{
    public class DeleteSpecificationMasterCommand :  IRequest<SpecificationMasterDTO>, IRequirePermission
    {
         public int Id { get; set; }    
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
