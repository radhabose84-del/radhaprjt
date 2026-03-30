using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.DeleteProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.TestData
{
    public static class ProjectMasterBuilders
    {
        public static CreateProjectMasterCommand ValidCreateCommand(
            string projectName = "Test Project",
            int unitId = 1,
            int departmentId = 1) =>
            new CreateProjectMasterCommand
            {
                Project = new CreateProjectMasterDto
                {
                    ProjectName = projectName,
                    ProjectTypeId = 1,
                    UnitId = unitId,
                    DepartmentId = departmentId,
                    BudgetAmount = 100000m,
                    BudgetYearId = 1,
                    CostCenterId = 1,
                    CurrencyId = 1,
                    ProjectCategoryId = 1,
                    AssetGroupId = 1,
                    PurposeRemarks = "Test purpose",
                    StartDate = DateTimeOffset.UtcNow,
                    EndDate = DateTimeOffset.UtcNow.AddMonths(6)
                }
            };

        public static UpdateProjectMasterCommand ValidUpdateCommand(
            int id = 1,
            string projectName = "Updated Project") =>
            new UpdateProjectMasterCommand
            {
                Project = new UpdateProjectMasterDto
                {
                    Id = id,
                    ProjectName = projectName,
                    ProjectTypeId = 1,
                    UnitId = 1,
                    DepartmentId = 1,
                    BudgetAmount = 100000m,
                    BudgetYearId = 1,
                    CostCenterId = 1,
                    CurrencyId = 1,
                    ProjectCategoryId = 1,
                    AssetGroupId = 1,
                    PurposeRemarks = "Updated purpose",
                    StartDate = DateTimeOffset.UtcNow,
                    EndDate = DateTimeOffset.UtcNow.AddMonths(6)
                }
            };

        public static DeleteProjectMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteProjectMasterCommand(id);

        public static GetProjectMasterDto ValidDto(
            int id = 1,
            string projectCode = "PRJ001",
            string projectName = "Test Project") =>
            new GetProjectMasterDto
            {
                Id = id,
                ProjectCode = projectCode,
                ProjectName = projectName,
                DepartmentId = 0,
                UnitId = 0,
                CurrencyId = 0,
                AssetGroupId = 0,
                BudgetYearId = 0,
                CostCenterId = 0,
                BudgetAmount = 100000m,
                PurposeRemarks = "Test purpose",
                Documents = new List<GetProjectDocumentDto>()
            };

        public static ProjectMasterAutoCompleteDto ValidAutoCompleteDto(
            int id = 1,
            string projectCode = "PRJ001",
            string projectName = "Test Project") =>
            new ProjectMasterAutoCompleteDto
            {
                Id = id,
                ProjectCode = projectCode,
                ProjectName = projectName,
                DepartmentId = 0,
                UnitId = 0,
                CurrencyId = 0,
                AssetGroupId = 0,
                BudgetYearId = 0,
                CostCenterId = 0
            };

        public static ProjectManagement.Domain.Entities.ProjectMaster ValidEntity(int id = 1) =>
            new ProjectManagement.Domain.Entities.ProjectMaster
            {
                Id = id,
                ProjectCode = "PRJ001",
                ProjectName = "Test Project",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
