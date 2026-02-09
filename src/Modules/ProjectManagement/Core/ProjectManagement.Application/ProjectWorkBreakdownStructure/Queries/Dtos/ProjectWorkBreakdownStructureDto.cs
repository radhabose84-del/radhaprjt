using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos
{
    public class ProjectWorkBreakdownStructureDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public string? ProjectCode { get; set; }

        public int? ParentWorkBreakdownStructureId { get; set; }
        public string? ParentWorkBreakdownStructureName { get; set; }

        public string? WorkBreakdownStructureName { get; set; }
        public string? WorkBreakdownStructureDescription { get; set; }

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int? DurationInDays { get; set; }

        public int ResponsibleDepartmentId { get; set; }
        public string? ResponsibleDepartment { get; set; } = default!;
        public string? ResponsiblePerson { get; set; } = default!;

        public int? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public decimal? PlannedBudgetAmount { get; set; }

        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }

        public bool IsMilestone { get; set; }
        public DateTimeOffset? MilestoneDate { get; set; }

        public string? Remarks { get; set; }
        public int StatusId { get; set; }
        public int Level { get; set; }

        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int BudgetYearId { get; set; }
        public string? BudgetYearName { get; set; }

        public bool IsActive { get; set; }

    }
}