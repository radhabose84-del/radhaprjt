using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain.Department
{
    public class DepartmentEntityTests
    {
        [Fact]
        public void Department_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserManagement.Domain.Entities.Department();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Department_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserManagement.Domain.Entities.Department();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Department_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserManagement.Domain.Entities.Department)).Should().BeTrue();
        }

        [Fact]
        public void Department_Properties_ShouldBeAssignable()
        {
            var entity = new UserManagement.Domain.Entities.Department
            {
                Id = 1,
                ShortName = "DEPT01",
                DeptName = "Test Department",
                CompanyId = 5,
                DepartmentGroupId = 3
            };

            entity.Id.Should().Be(1);
            entity.ShortName.Should().Be("DEPT01");
            entity.DeptName.Should().Be("Test Department");
            entity.CompanyId.Should().Be(5);
            entity.DepartmentGroupId.Should().Be(3);
        }

        [Fact]
        public void Department_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserManagement.Domain.Entities.Department
            {
                ShortName = null,
                DeptName = null,
                DepartmentGroup = null,
                UserDepartments = null
            };

            entity.ShortName.Should().BeNull();
            entity.DeptName.Should().BeNull();
            entity.DepartmentGroup.Should().BeNull();
            entity.UserDepartments.Should().BeNull();
        }

        [Fact]
        public void Department_NavigationProperty_DepartmentGroup_ShouldBeAssignable()
        {
            var departmentGroup = new DepartmentGroup
            {
                Id = 1,
                DepartmentGroupCode = "GRP01",
                DepartmentGroupName = "Test Group"
            };

            var entity = new UserManagement.Domain.Entities.Department
            {
                Id = 1,
                DepartmentGroupId = 1,
                DepartmentGroup = departmentGroup
            };

            entity.DepartmentGroup.Should().NotBeNull();
            entity.DepartmentGroup!.DepartmentGroupName.Should().Be("Test Group");
        }

        [Fact]
        public void Department_AuditFields_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new UserManagement.Domain.Entities.Department
            {
                CreatedBy = 1,
                CreatedAt = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedAt = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "127.0.0.2"
            };

            entity.CreatedBy.Should().Be(1);
            entity.CreatedAt.Should().Be(now);
            entity.ModifiedBy.Should().Be(2);
            entity.ModifiedAt.Should().Be(now.AddHours(1));
        }
    }
}
