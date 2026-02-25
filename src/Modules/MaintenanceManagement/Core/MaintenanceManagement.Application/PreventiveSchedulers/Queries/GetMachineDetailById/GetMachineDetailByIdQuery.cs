using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class GetMachineDetailByIdQuery : IRequest<ApiResponseDTO<PreventiveSchedulerDto>>
    {
        public int Id { get; set; }
    }
}