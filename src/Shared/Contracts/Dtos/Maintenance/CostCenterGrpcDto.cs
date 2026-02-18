using System;

namespace Contracts.Dtos.Maintenance
{
    public class CostCenterGrpcDto
    {
        public int Id { get; set; }
        public string CostCenterCode { get; set; } = default!;
        public string CostCenterName { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public DateTimeOffset EffectiveDate { get; set; }
        public string ResponsiblePerson { get; set; } = default!;
        public decimal? BudgetAllocated { get; set; }
        public string Remarks { get; set; } = default!;
        public int IsActive { get; set; }
    }
}
