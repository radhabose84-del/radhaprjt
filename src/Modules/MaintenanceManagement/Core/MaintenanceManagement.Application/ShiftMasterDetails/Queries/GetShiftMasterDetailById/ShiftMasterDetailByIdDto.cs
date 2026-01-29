using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetailById
{
    public class ShiftMasterDetailByIdDto
    {
        public int Id { get; set; }
        public int ShiftMasterId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Decimal DurationInHours { get; set; }
        public int BreakDurationInMinutes { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int ShiftSupervisorId { get; set; }
    }
}