using Contracts.Common;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup;
using ProjectManagement.Domain.Common;

namespace ProjectManagement.UnitTests.TestData
{
    public static class ProjectWorkBreakdownStructureBuilders
    {
        public static CreateProjectWorkBreakdownStructureCommand ValidCreateCommand(
            int projectId = 1,
            string wbsName = "Foundation Work",
            int currencyId = 1,
            int departmentId = 1,
            string responsiblePerson = "John Doe") =>
            new CreateProjectWorkBreakdownStructureCommand
            {
                ProjectId = projectId,
                WorkBreakdownStructureName = wbsName,
                WorkBreakdownStructureDescription = "Test WBS Description",
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(30),
                ResponsibleDepartmentId = departmentId,
                ResponsiblePerson = responsiblePerson,
                CurrencyId = currencyId,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                IsMilestone = false,
                PlannedBudgetAmount = 1000m
            };

        public static UpdateProjectWorkBreakdownStructureCommand ValidUpdateCommand(
            int id = 1,
            int projectId = 1,
            string wbsName = "Foundation Work Updated",
            int currencyId = 1,
            int departmentId = 1) =>
            new UpdateProjectWorkBreakdownStructureCommand
            {
                Id = id,
                ProjectId = projectId,
                WorkBreakdownStructureName = wbsName,
                WorkBreakdownStructureDescription = "Updated Description",
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(30),
                ResponsibleDepartmentId = departmentId,
                ResponsiblePerson = "Jane Doe",
                CurrencyId = currencyId,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                IsMilestone = false
            };

        public static DeleteProjectWorkBreakdownStructureCommand ValidDeleteCommand(int id = 1) =>
            new DeleteProjectWorkBreakdownStructureCommand(id);

        public static ProjectWorkBreakdownStructureDto ValidDto(
            int id = 1,
            int projectId = 1,
            string wbsName = "Foundation Work") =>
            new ProjectWorkBreakdownStructureDto
            {
                Id = id,
                ProjectId = projectId,
                WorkBreakdownStructureName = wbsName,
                ResponsibleDepartmentId = 1,
                ResponsiblePerson = "John Doe",
                CurrencyId = 1,
                CurrencyName = "USD",
                UnitId = 1,
                UnitName = "HQ",
                BudgetYearId = 1,
                BudgetYearName = "FY2025",
                Level = 1,
                StatusId = 1,
                IsActive = true
            };

        public static List<ProjectWorkBreakdownStructureDto> ValidDtoList(int count = 2)
        {
            var list = new List<ProjectWorkBreakdownStructureDto>();
            for (int i = 1; i <= count; i++)
                list.Add(ValidDto(id: i, wbsName: $"WBS Task {i}"));
            return list;
        }

        public static List<ProjectWorkBreakdownStructureAutocompleteDto> ValidAutocompleteList() =>
            new List<ProjectWorkBreakdownStructureAutocompleteDto>
            {
                new ProjectWorkBreakdownStructureAutocompleteDto { Id = 1, WorkBreakdownStructureName = "Foundation Work" },
                new ProjectWorkBreakdownStructureAutocompleteDto { Id = 2, WorkBreakdownStructureName = "Electrical Work" }
            };

        public static List<ProjectWbsLookupDto> ValidLookupList() =>
            new List<ProjectWbsLookupDto>
            {
                new ProjectWbsLookupDto { Id = 1, WorkBreakdownStructureName = "Foundation Work" },
                new ProjectWbsLookupDto { Id = 2, WorkBreakdownStructureName = "Electrical Work" }
            };

        public static ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure ValidEntity(
            int id = 1,
            int projectId = 1) =>
            new ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure
            {
                Id = id,
                ProjectId = projectId,
                WorkBreakdownStructureName = "Foundation Work",
                ResponsibleDepartmentId = 1,
                ResponsiblePerson = "John Doe",
                CurrencyId = 1,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                Level = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
