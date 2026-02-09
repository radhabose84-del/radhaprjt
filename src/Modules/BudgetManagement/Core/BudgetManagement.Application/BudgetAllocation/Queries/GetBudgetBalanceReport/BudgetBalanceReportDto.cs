using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport
{
    public class BudgetBalanceReportDto
    {
        public int UnitId { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }

        public int RequestById { get; set; }
        public string? RequestType { get; set; }

        public int RequestMonthId { get; set; }
        public string? RequestMonth { get; set; }

        public int BudgetGroupId { get; set; }
        public string? BudgetGroupName { get; set; }

        public int AllocationTypeId { get; set; }
        public string? AllocationType { get; set; }

        public decimal ApprovedAmount { get; set; }
        public decimal RemainingBalance { get; set; }

        // ✅ Calculated
        public decimal UtilizedAmount { get; set; }
    }
}