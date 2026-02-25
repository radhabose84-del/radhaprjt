using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class GetPreventiveSchedulerByIdQuery : IRequest<ApiResponseDTO<PreventiveSchedulerHdrByIdDto>>
    {
        public int Id { get; set; }
    }
}