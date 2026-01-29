using System;

namespace Contracts.Dtos.Maintenance
{
    public class CostCenterGrpcDto
    {
        public int Id { get; set; }
        public string CostCenterCode { get; set; }
        public string CostCenterName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public string ResponsiblePerson { get; set; }
        public decimal? BudgetAllocated { get; set; }
        public string Remarks { get; set; }        
        public int IsActive { get; set; }
    }
}
