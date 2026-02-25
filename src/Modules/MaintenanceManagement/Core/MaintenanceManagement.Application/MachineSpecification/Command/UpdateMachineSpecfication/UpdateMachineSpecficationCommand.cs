using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication
{
    public class UpdateMachineSpecficationCommand : IRequest<ApiResponseDTO<bool>>
    {
        public List<MachineSpecificationUpdateDto>? Specifications { get; set; }
    }
    public class MachineSpecificationUpdateDto
    {
        public int SpecificationId { get; set; }
        public string? SpecificationValue { get; set; }
        public int MachineId { get; set; }
    
    }
}