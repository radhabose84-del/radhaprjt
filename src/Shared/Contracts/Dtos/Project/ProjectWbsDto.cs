namespace Contracts.Dtos.Project
{
    public class ProjectWbsDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public int? ParentWorkBreakdownStructureId { get; set; }

        public string WorkBreakdownStructureName { get; set; } = string.Empty;
        public string? WorkBreakdownStructureDescription { get; set; }

        // store as DateTimeOffset since your DB has +00:00 / +05:30
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public int? DurationInDays { get; set; }

        public int? ResponsibleDepartmentId { get; set; }
        public string? ResponsiblePerson { get; set; }

        public int? CostCenterId { get; set; }

        public decimal? PlannedBudgetAmount { get; set; }
        public int? CurrencyId { get; set; }

        public bool IsMilestone { get; set; }
        public DateTimeOffset? MilestoneDate { get; set; }

        public string? Remarks { get; set; }

        public int? StatusId { get; set; }
        public int? Level { get; set; }
        public int? UnitId { get; set; }
        public int? BudgetYearId { get; set; }
        public bool IsActive { get; set; }

       
    }
}