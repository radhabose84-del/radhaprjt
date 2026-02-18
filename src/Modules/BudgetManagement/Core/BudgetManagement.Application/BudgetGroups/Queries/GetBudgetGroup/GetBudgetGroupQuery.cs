using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.BudgetGroups;
using Contracts.Common;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroup
{
    public class GetBudgetGroupQuery : IRequest<ApiResponseDTO<List<BudgetGroupListItemDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        public string? SearchTerm { get; set; }

        public int? UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public int? CostCenterId { get; set; }
        public int? ParentBudgetGroupId { get; set; }
        public bool? IsActive { get; set; }
        public int? AllocationRuleId { get; set; }
        public int? BudgetTypeId { get; set; }
        public bool? CarryForward { get; set; }
    }
}