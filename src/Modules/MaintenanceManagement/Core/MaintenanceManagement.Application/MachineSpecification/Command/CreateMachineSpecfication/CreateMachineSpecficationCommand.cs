using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication
{
    public class CreateMachineSpecficationCommand : IRequest<ApiResponseDTO<List<int>>>, IRequirePermission
    {
    public List<MachineSpecificationCreateDto>? Specifications { get; set; }
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
    public class MachineSpecificationCreateDto
    {
        public int SpecificationId { get; set; }
        public string? SpecificationValue { get; set; }
        public int MachineId { get; set; }
    
    }
}
