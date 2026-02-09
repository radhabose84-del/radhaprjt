using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleDetailsMonthwise
{
    public class GetSpindleDetailsMonthwiseDto
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int BudgetGroupId { get; set; }
        public string? BudgetGroupName { get; set; }
        public int? BudgetSubGroupId { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public int AllocationTypeId { get; set; }
        public string? AllocationTypeName { get; set; }
        public int SpindleCount { get; set; }
        public decimal RatePerSpindle { get; set; }
        public decimal ApprovedAmount { get; set; }

    }
}