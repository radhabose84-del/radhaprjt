using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup
{
    public class GetMachineGroupQuery :IRequest<ApiResponseDTO<List<MachineGroupDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }


    }
}