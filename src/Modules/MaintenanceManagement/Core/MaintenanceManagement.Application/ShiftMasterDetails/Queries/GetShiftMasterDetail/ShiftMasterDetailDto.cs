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
        public string ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DurationInHours { get; set; }
        public string BreakDurationInMinutes { get; set; }
        public string EffectiveDate { get; set; }
        public int ShiftSupervisorId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}