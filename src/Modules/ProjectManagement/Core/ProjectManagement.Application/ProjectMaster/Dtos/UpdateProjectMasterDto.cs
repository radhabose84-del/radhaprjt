using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;

namespace ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster
{
    public class UpdateProjectMasterDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = default!;
        public string? ProjectDescription { get; set; }

        public int ProjectTypeId { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }

        // Financial
        public decimal BudgetAmount { get; set; }
        public int BudgetYearId { get; set; }
        public int CostCenterId { get; set; }
        public int CurrencyId { get; set; }

        // Timeline
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        // Capex / Asset
        public int ProjectCategoryId { get; set; }
        public int AssetGroupId { get; set; }

        public string PurposeRemarks { get; set; } = default!;        
        public List<ProjectDocumentDto> Documents { get; set; } = new();
    }
}