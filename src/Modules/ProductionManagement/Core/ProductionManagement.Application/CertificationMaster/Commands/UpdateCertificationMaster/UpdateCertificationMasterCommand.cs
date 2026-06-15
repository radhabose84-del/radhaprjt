using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster
{
    public class UpdateCertificationMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? CertificationName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
