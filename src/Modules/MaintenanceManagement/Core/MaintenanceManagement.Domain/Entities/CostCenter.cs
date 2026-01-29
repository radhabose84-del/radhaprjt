using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class CostCenter : BaseEntity
    {
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public decimal? BudgetAllocated { get; set; }
        public string? Remarks { get; set; }
        public ICollection<MachineMaster>? MachineMasters { get; set; } 

    }
}