using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;
using Contracts.Common;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand
{
    public class UpdateProjectWorkBreakdownStructureCommand : IRequest<ProjectWorkBreakdownStructureDto>, IRequirePermission
    {
         public int Id { get; set; }                 // WBS Id (mandatory)

        public int ProjectId { get; set; }          
        public int? ParentWorkBreakdownStructureId { get; set; }
        
        public string WorkBreakdownStructureName { get; set; } = default!;
        public string? WorkBreakdownStructureDescription { get; set; }
       
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }        
        public int ResponsibleDepartmentId { get; set; }
        public string ResponsiblePerson { get; set; } = default!;
 
        public int? CostCenterId { get; set; }
        public decimal? PlannedBudgetAmount { get; set; }      
        public int CurrencyId { get; set; }
        public bool IsMilestone { get; set; }
        public DateTimeOffset? MilestoneDate { get; set; }        
        public string? Remarks { get; set; }      
        public int StatusId { get; set; }

        // These will normally be inherited from Project, but kept for DTO mapping
        public int UnitId { get; set; }
        public int BudgetYearId { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
