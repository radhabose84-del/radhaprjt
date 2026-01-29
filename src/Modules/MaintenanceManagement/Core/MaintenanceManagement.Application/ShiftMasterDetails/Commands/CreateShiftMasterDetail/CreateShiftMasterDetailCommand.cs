using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail
{
    public class CreateShiftMasterDetailCommand : IRequest<ApiResponseDTO<int>>
    {
        public int ShiftMasterId { get; set; }
        public int UnitId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int BreakDurationInMinutes { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int ShiftSupervisorId { get; set; }
        
    }
}