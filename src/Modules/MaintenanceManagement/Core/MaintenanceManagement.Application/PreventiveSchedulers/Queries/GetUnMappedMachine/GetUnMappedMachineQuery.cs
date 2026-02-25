using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine
{
    public class GetUnMappedMachineQuery : IRequest<ApiResponseDTO<List<UnMappedMachineDto>>>
    {
        public int Id { get; set; }
    }
}