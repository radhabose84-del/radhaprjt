using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineNoDepartmentbyId
{
    public class GetMachineNoDepartmentbyIdQuery : IRequest<List<GetMachineNoDepartmentbyIdDto>>
    {
        public int DepartmentId { get; set; }
    }
}