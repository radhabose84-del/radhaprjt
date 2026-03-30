using AutoMapper;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class DepartmentProfileTests
    {
        private readonly IMapper _mapper;

        public DepartmentProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DepartmentProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateDepartmentCommand_MapsTo_Department_WithActiveAndNotDeleted()
        {
            var command = new CreateDepartmentCommand
            {
                ShortName = "DEPT01",
                DeptName = "Test Department",
                CompanyId = 1,
                DepartmentGroupId = 2
            };

            var entity = _mapper.Map<Department>(command);

            entity.ShortName.Should().Be("DEPT01");
            entity.DeptName.Should().Be("Test Department");
            entity.CompanyId.Should().Be(1);
            entity.DepartmentGroupId.Should().Be(2);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateDepartmentCommand_MapsTo_Department()
        {
            var command = new UpdateDepartmentCommand
            {
                Id = 1,
                ShortName = "UPD01",
                DeptName = "Updated Department",
                CompanyId = 2,
                DepartmentGroupId = 3,
                IsActive = Status.Active
            };

            var entity = _mapper.Map<Department>(command);

            entity.Id.Should().Be(1);
            entity.ShortName.Should().Be("UPD01");
            entity.DeptName.Should().Be("Updated Department");
            entity.CompanyId.Should().Be(2);
            entity.DepartmentGroupId.Should().Be(3);
        }

        [Fact]
        public void DeleteDepartmentCommand_MapsTo_Department_WithDeleted()
        {
            var command = new DeleteDepartmentCommand { Id = 1 };

            var entity = _mapper.Map<Department>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void Department_MapsTo_DepartmentAutocompleteDto()
        {
            var entity = new Department
            {
                Id = 1,
                DeptName = "Test Department",
                ShortName = "DEPT01"
            };

            var dto = _mapper.Map<UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch.DepartmentAutocompleteDto>(entity);

            dto.Id.Should().Be(1);
            dto.DeptName.Should().Be("Test Department");
        }
    }
}
