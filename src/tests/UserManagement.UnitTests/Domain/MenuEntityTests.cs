using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class MenuEntityTests
    {
        [Fact]
        public void Menu_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Menu();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Menu_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Menu();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Menu_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Menu)).Should().BeTrue();
        }

        [Fact]
        public void Menu_ChildMenusCollection_DefaultsToEmpty()
        {
            var entity = new Menu();

            entity.ChildMenus.Should().NotBeNull();
            entity.ChildMenus.Should().BeEmpty();
        }

        [Fact]
        public void Menu_Properties_ShouldBeAssignable()
        {
            var entity = new Menu
            {
                Id = 1,
                MenuName = "Dashboard",
                ModuleId = 5,
                ParentId = 0,
                MenuUrl = "/dashboard",
                MenuIcon = "icon-home",
                SortOrder = 1,
                Type = "menu"
            };

            entity.Id.Should().Be(1);
            entity.MenuName.Should().Be("Dashboard");
            entity.ModuleId.Should().Be(5);
            entity.ParentId.Should().Be(0);
            entity.MenuUrl.Should().Be("/dashboard");
            entity.MenuIcon.Should().Be("icon-home");
            entity.SortOrder.Should().Be(1);
            entity.Type.Should().Be("menu");
        }

        [Fact]
        public void Menu_NullableNavigation_ShouldAcceptNull()
        {
            var entity = new Menu
            {
                MenuName = null,
                MenuUrl = null,
                MenuIcon = null,
                Type = null,
                Parent = null,
                Module = null,
                RoleMenus = null,
                RoleParents = null,
                RoleChildren = null,
                CustomFieldMenus = null
            };

            entity.MenuName.Should().BeNull();
            entity.MenuUrl.Should().BeNull();
            entity.Parent.Should().BeNull();
            entity.Module.Should().BeNull();
        }

        [Fact]
        public void Menu_NavigationProperty_Parent_ShouldBeAssignable()
        {
            var parent = new Menu { Id = 1, MenuName = "Parent" };
            var child = new Menu
            {
                Id = 2,
                MenuName = "Child",
                ParentId = 1,
                Parent = parent
            };

            child.Parent.Should().NotBeNull();
            child.Parent!.MenuName.Should().Be("Parent");
        }

        [Fact]
        public void Menu_ChildMenus_ShouldBeAssignable()
        {
            var parent = new Menu
            {
                Id = 1,
                ChildMenus = new List<Menu>
                {
                    new Menu { Id = 2 },
                    new Menu { Id = 3 }
                }
            };

            parent.ChildMenus.Should().HaveCount(2);
        }
    }
}
