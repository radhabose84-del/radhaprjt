namespace Contracts.Dtos.Project
{
    public class ProjectMasterDto
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string? ProjectDescription { get; set; }

        public int ProjectTypeId { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }

        public decimal BudgetAmount { get; set; }
        public int BudgetYearId { get; set; }
        public int CostCenterId { get; set; }
        public int CurrencyId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int ProjectCategoryId { get; set; }
        public int AssetGroupId { get; set; }
        public string PurposeRemarks { get; set; } = default!;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int StatusId { get; set; }           
    }
}