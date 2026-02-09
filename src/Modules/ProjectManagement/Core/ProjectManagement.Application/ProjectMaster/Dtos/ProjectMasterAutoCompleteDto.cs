using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete
{
    public class ProjectMasterAutoCompleteDto
    {
          public int Id { get; set; }
        public string ProjectCode { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string? ProjectDescription { get; set; }
        public string? ProjectType { get; set;}
        public string? ProjectTypeDescription { get; set; }      
        public Decimal? BudgetAmount { get; set; }     
        public  int BudgetYearId { get; set; }        
        public string? BudgetYearName { get; set;}
        public int CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int AssetGroupId { get; set; }         
        public string? AssetGroupName { get; set; }
        
        public int StatusId { get; set; }
    }
}