using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser
{
    public class GetMachineGroupUserQuery  : IRequest<ApiResponseDTO<List<MachineGroupUserDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}