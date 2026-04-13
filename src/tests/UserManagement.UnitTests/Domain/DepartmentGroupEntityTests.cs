using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class DepartmentGroupEntityTests
    {
        [Fact]
        public void DepartmentGroup_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new DepartmentGroup();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DepartmentGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DepartmentGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DepartmentGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DepartmentGroup)).Should().BeTrue();
        }

        [Fact]
        public void DepartmentGroup_Properties_ShouldBeAssignable()
        {
            var entity = new DepartmentGroup
            {
                Id = 1,
                DepartmentGroupCode = "DG01",
                DepartmentGroupName = "Test Group"
            };

            entity.Id.Should().Be(1);
            entity.DepartmentGroupCode.Should().Be("DG01");
            entity.DepartmentGroupName.Should().Be("Test Group");
        }

        [Fact]
        public void DepartmentGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DepartmentGroup
            {
                DepartmentGroupCode = null,
                DepartmentGroupName = null,
                Departments = null
            };

            entity.DepartmentGroupCode.Should().BeNull();
            entity.DepartmentGroupName.Should().BeNull();
            entity.Departments.Should().BeNull();
        }

        [Fact]
        public void DepartmentGroup_NavigationProperty_Departments_ShouldBeAssignable()
        {
            var departments = new List<UserManagement.Domain.Entities.Department>
            {
                new UserManagement.Domain.Entities.Department { Id = 1 },
                new UserManagement.Domain.Entities.Department { Id = 2 }
            };

            var entity = new DepartmentGroup { Departments = departments };

            entity.Departments.Should().NotBeNull();
            entity.Departments.Should().HaveCount(2);
        }
    }
}
