using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster
{
    public class CreateShiftMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string ShiftCode { get; set; } = default!;
        public string ShiftName { get; set; } = default!;
        public DateOnly EffectiveDate { get; set; }
    }
}