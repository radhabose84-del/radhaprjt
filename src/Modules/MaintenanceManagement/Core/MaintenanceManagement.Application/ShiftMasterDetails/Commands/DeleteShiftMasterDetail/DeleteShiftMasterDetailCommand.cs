using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail
{
    public class DeleteShiftMasterDetailCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
    }
}