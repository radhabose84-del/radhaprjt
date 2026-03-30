using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class DivisionBuilders
    {
        public static CreateDivisionCommand ValidCreateCommand(
            string? shortName = "DIV01",
            string? name = "Test Division",
            int companyId = 1) =>
            new CreateDivisionCommand
            {
                ShortName = shortName,
                Name = name,
                CompanyId = companyId
            };

        public static UpdateDivisionCommand ValidUpdateCommand(
            int id = 1,
            string shortName = "DIV01",
            string name = "Updated Division",
            int companyId = 1,
            byte isActive = 1) =>
            new UpdateDivisionCommand
            {
                Id = id,
                ShortName = shortName,
                Name = name,
                CompanyId = companyId,
                IsActive = isActive
            };

        public static DeleteDivisionCommand ValidDeleteCommand(int id = 1) =>
            new DeleteDivisionCommand { Id = id };

        public static DivisionDTO ValidDto(
            int id = 1,
            string? shortName = "DIV01",
            string? name = "Test Division",
            int companyId = 1) =>
            new DivisionDTO
            {
                Id = id,
                ShortName = shortName,
                Name = name,
                CompanyId = companyId,
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static Division ValidEntity(
            int id = 1,
            string? shortName = "DIV01",
            string? name = "Test Division",
            int companyId = 1) =>
            new Division
            {
                Id = id,
                ShortName = shortName,
                Name = name,
                CompanyId = companyId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static List<DivisionAutoCompleteDTO> ValidAutoCompleteList() =>
            new List<DivisionAutoCompleteDTO>
            {
                new DivisionAutoCompleteDTO { Id = 1, Name = "Test Division" }
            };
    }
}
