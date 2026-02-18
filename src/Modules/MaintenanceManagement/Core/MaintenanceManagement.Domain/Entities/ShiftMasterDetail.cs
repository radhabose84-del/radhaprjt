using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class ShiftMasterDetail : BaseEntity
    {
        public new int Id { get; set; }
        public int ShiftMasterId { get; set; }
        public ShiftMaster ShiftMaster { get; set; } = default!;
        public int UnitId { get; set; }
        public TimeOnly  StartTime { get; set; }
        public TimeOnly  EndTime { get; set; }
        public Decimal DurationInHours { get; set; }
        public int BreakDurationInMinutes { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int ShiftSupervisorId { get; set; }


    }
}