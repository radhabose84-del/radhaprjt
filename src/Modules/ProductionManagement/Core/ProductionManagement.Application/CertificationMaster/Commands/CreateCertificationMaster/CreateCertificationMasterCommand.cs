using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster
{
    public class CreateCertificationMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? CertificationName { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
