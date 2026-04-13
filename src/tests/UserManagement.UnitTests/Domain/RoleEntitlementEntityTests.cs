using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class RoleEntitlementEntityTests
    {
        [Fact]
        public void RoleEntitlement_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new RoleEntitlement();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void RoleEntitlement_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RoleEntitlement();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RoleEntitlement_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RoleEntitlement)).Should().BeTrue();
        }

        [Fact]
        public void RoleEntitlement_Properties_ShouldBeAssignable()
        {
            var entity = new RoleEntitlement
            {
                Id = 1,
                UserRoleId = 5,
                MenuId = 10,
                CanView = true,
                CanAdd = true,
                CanUpdate = false,
                CanDelete = false,
                CanExport = true,
                CanApprove = false
            };

            entity.Id.Should().Be(1);
            entity.UserRoleId.Should().Be(5);
            entity.MenuId.Should().Be(10);
            entity.CanView.Should().BeTrue();
            entity.CanAdd.Should().BeTrue();
            entity.CanUpdate.Should().BeFalse();
            entity.CanDelete.Should().BeFalse();
            entity.CanExport.Should().BeTrue();
            entity.CanApprove.Should().BeFalse();
        }

        [Fact]
        public void RoleEntitlement_DefaultBooleans_ShouldBeFalse()
        {
            var entity = new RoleEntitlement();

            entity.CanView.Should().BeFalse();
            entity.CanAdd.Should().BeFalse();
            entity.CanUpdate.Should().BeFalse();
            entity.CanDelete.Should().BeFalse();
            entity.CanExport.Should().BeFalse();
            entity.CanApprove.Should().BeFalse();
        }

        [Fact]
        public void RoleEntitlement_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new RoleEntitlement
            {
                UserRole = null,
                Menu = null
            };

            entity.UserRole.Should().BeNull();
            entity.Menu.Should().BeNull();
        }

        [Fact]
        public void RoleEntitlement_NavigationProperty_Menu_ShouldBeAssignable()
        {
            var menu = new Menu { Id = 10, MenuName = "Dashboard" };
            var entity = new RoleEntitlement { MenuId = 10, Menu = menu };

            entity.Menu.Should().NotBeNull();
            entity.Menu!.MenuName.Should().Be("Dashboard");
        }
    }
}
