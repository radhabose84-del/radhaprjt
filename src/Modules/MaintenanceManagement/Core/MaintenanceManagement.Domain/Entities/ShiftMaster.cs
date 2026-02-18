using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class ShiftMaster : BaseEntity
    {
        public new int Id { get; set; }
        public string ShiftCode { get; set; } = default!;
        public string ShiftName { get; set; } = default!;
        public DateOnly EffectiveDate { get; set; }
        public List<ShiftMasterDetail> ShiftMasterDetails { get; set; } = default!;
        public ICollection<MachineMaster>? MachineMasters { get; set; }  
    }
}