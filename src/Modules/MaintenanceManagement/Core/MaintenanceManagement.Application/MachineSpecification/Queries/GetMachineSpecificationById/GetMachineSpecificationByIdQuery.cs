using Contracts.Common;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Queries.GetMachineSpecificationById
{
    public class GetMachineSpecificationByIdQuery : IRequest<ApiResponseDTO<List<MachineSpecificationDto>>>
    {
        public int Id { get; set; }
    }
}