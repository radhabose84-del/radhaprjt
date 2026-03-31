using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class DepartmentBuilders
    {
        public static CreateDepartmentCommand ValidCreateCommand(
            string? shortName = "DEPT01",
            string? deptName = "Test Department",
            int companyId = 1,
            int departmentGroupId = 1) =>
            new CreateDepartmentCommand
            {
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = departmentGroupId
            };

        public static UpdateDepartmentCommand ValidUpdateCommand(
            int id = 1,
            string? shortName = "DEPT01",
            string? deptName = "Updated Department",
            int companyId = 1,
            int departmentGroupId = 1,
            Status isActive = Status.Active) =>
            new UpdateDepartmentCommand
            {
                Id = id,
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = departmentGroupId,
                IsActive = isActive
            };

        public static DeleteDepartmentCommand ValidDeleteCommand(int id = 1) =>
            new DeleteDepartmentCommand { Id = id };

        public static DepartmentDto ValidDto(
            int id = 1,
            string? shortName = "DEPT01",
            string? deptName = "Test Department",
            int companyId = 1,
            int departmentGroupId = 1) =>
            new DepartmentDto
            {
                Id = id,
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = departmentGroupId,
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static GetDepartmentDto ValidGetDepartmentDto(
            int id = 1,
            string? shortName = "DEPT01",
            string? deptName = "Test Department",
            int companyId = 1,
            int departmentGroupId = 1) =>
            new GetDepartmentDto
            {
                Id = id,
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = departmentGroupId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static Department ValidEntity(
            int id = 1,
            string? shortName = "DEPT01",
            string? deptName = "Test Department",
            int companyId = 1,
            int departmentGroupId = 1) =>
            new Department
            {
                Id = id,
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = departmentGroupId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static List<DepartmentAutocompleteDto> ValidAutoCompleteList() =>
            new List<DepartmentAutocompleteDto>
            {
                new DepartmentAutocompleteDto { Id = 1, DeptName = "Test Department" }
            };
    }
}
