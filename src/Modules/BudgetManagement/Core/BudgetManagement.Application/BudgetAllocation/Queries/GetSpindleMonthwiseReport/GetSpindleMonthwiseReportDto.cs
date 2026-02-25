namespace BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport
{
    public class GetSpindleMonthwiseReportDto
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }

        public int BudgetGroupId { get; set; }
        public string? BudgetGroup { get; set; }

        public int RequestById { get; set; }
        public string? RequestBy { get; set; }

        public int RequestMonthId { get; set; }
        public string? MonthCode { get; set; }
        public string? RequestMonth { get; set; }

        public int AllocationTypeId { get; set; }
        public string? AllocationType { get; set; }

        public decimal SpindleCount { get; set; }
        public decimal RatePerSpindle { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int DepartmentId { get; set; }
        public string? Department { get; set; }
        public int CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public int CurrencyId { get; set; }
        public string? Currency { get; set; }
        public decimal RemainingBalance { get; set; }
        public int RequestId { get; set; }
        public int ProjectId { get; set; }
        public int WBSId { get; set; }
        
    }
}