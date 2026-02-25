using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail
{
    public class UpdateShiftMasterDetailCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
        public int ShiftMasterId { get; set; }
        public int UnitId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int BreakDurationInMinutes { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int ShiftSupervisorId { get; set; }
        public byte IsActive { get; set; }
    }
}