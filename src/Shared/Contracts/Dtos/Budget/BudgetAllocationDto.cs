using System;

namespace Contracts.Dtos.Budget
{
    public class BudgetAllocationDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public string FinancialYearName { get; set; } = string.Empty;
        public int BudgetGroupId { get; set; }
        public string BudgetGroup { get; set; } = string.Empty;
        public int RequestById { get; set; }
        public string RequestBy { get; set; } = string.Empty;
        public int RequestMonthId { get; set; }
        public string MonthCode { get; set; } = string.Empty;
        public string RequestMonth { get; set; } = string.Empty;
        public int AllocationTypeId { get; set; }
        public string AllocationType { get; set; } = string.Empty;
        public decimal SpindleCount { get; set; }
        public decimal RatePerSpindle { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; } = string.Empty;
        public int CostCenterId { get; set; }
        public string CostCenter { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal RemainingBalance { get; set; }
        public int RequestId { get; set; }
        public int ProjectId { get; set; }
        public int WBSId { get; set; }
    }
}
