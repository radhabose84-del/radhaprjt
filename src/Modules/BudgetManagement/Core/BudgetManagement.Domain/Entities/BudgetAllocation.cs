using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Domain.Common;

namespace BudgetManagement.Domain.Entities
{
    public class BudgetAllocation : BaseEntity, IActivityTracked
    {
        public int FinancialYearId { get; set; }
        public int RequestById { get; set; }
        public MiscMaster? RequestMonthYearType { get; set; }
        public int? RequestMonthId { get; set; }
        public MiscMaster? RequestMonthType { get; set; }
        public int UnitId { get; set; }
        public int? RequestId { get; set; }
        public BudgetRequest? BudgetRequestType { get; set; }
        public int? BudgetGroupId { get; set; }
        public BudgetGroup? BudgetGroupType { get; set; }
        public int? BudgetSubGroupId { get; set; }
        public int AllocationTypeId { get; set; }
        public MiscMaster? AllocationRuleType { get; set; }
        public int? SpindleCount { get; set; }
        public decimal? RatePerSpindle { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string? Remarks { get; set; }
        public decimal? RemainingBalance { get; set; }
        public int? ProjectId { get; set; }
	    public int? WBSId { get; set; }
    }
}