using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand
{
    public class CreateProjectWorkBreakdownStructureCommand    : IRequest<ProjectWorkBreakdownStructureDto>
    {
         public int ProjectId { get; set; }
        public int? ParentWorkBreakdownStructureId { get; set; }

        // Basic Info
        public string WorkBreakdownStructureName { get; set; } = default!;
        public string? WorkBreakdownStructureDescription { get; set; }

        // Dates
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        // Department & Responsibility
        public int ResponsibleDepartmentId { get; set; }
        public string ResponsiblePerson { get; set; } = default!;

        // Costing & Budget
        public int? CostCenterId { get; set; }
        public decimal? PlannedBudgetAmount { get; set; }

        // Currency
        public int CurrencyId { get; set; }

        // Milestone
        public bool IsMilestone { get; set; }
        public DateTimeOffset? MilestoneDate { get; set; }

        // Remarks
        public string? Remarks { get; set; }

        // Status (Default usually 1)
        public int StatusId { get; set; } = 1;

        // Level (calculated in handler, not from client but kept optional)
        public int? Level { get; set; }

        // Unit & Budget Year
        public int UnitId { get; set; }
        public int BudgetYearId { get; set; }
    }
}