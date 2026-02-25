namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster
{
    public class GetProjectMasterDto
    {

        public int Id { get; set; }
        public string ProjectCode { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string? ProjectDescription { get; set; }

        public int ProjectTypeId { get; set; }
        public string? ProjectType { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public decimal BudgetAmount { get; set; }
        public int BudgetYearId { get; set; }
        public string? BudgetYearName { get; set; }   // from FinancialYear gRPC
        public int CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int ProjectCategoryId { get; set; }
        public string? ProjectCategory { get; set; }
        public int AssetGroupId { get; set; }
        public string? AssetGroup { get; set; }
        public string PurposeRemarks { get; set; } = default!;        
        

        public List<GetProjectDocumentDto> Documents { get; set; } = new List<GetProjectDocumentDto>(); // GetProjectDocumentDto

    }
}