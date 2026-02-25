using Contracts.Common;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterById
{
    public class GetShiftMasterByIdQuery : IRequest<ApiResponseDTO<ShiftMasterDTO>>
    {
        public int Id { get; set; }
    }
}