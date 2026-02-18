using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetail
{
    public class ShiftMasterDetailDto
    {
        
        public int ShiftMasterId { get; set; }
        public int Id { get; set; }
        public string ShiftCode { get; set; } = default!;
        public string ShiftName { get; set; } = default!;
        public string StartTime { get; set; } = default!;
        public string EndTime { get; set; } = default!;
        public string DurationInHours { get; set; } = default!;
        public string BreakDurationInMinutes { get; set; } = default!;
        public string EffectiveDate { get; set; } = default!;
        public int ShiftSupervisorId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}