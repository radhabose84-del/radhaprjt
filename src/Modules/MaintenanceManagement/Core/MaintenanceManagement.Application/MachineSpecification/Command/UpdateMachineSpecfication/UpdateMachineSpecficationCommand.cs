using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication
{
    public class UpdateMachineSpecficationCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public List<MachineSpecificationUpdateDto>? Specifications { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
    public class MachineSpecificationUpdateDto
    {
        public int SpecificationId { get; set; }
        public string? SpecificationValue { get; set; }
        public int MachineId { get; set; }
    
    }
}
