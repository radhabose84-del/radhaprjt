using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster
{
    public class DeleteShiftMasterCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
    }
}