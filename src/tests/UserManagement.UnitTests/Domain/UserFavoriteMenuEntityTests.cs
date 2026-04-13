using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class UserFavoriteMenuEntityTests
    {
        [Fact]
        public void UserFavoriteMenu_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserFavoriteMenu();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UserFavoriteMenu_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserFavoriteMenu();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UserFavoriteMenu_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserFavoriteMenu)).Should().BeTrue();
        }

        [Fact]
        public void UserFavoriteMenu_Properties_ShouldBeAssignable()
        {
            var entity = new UserFavoriteMenu
            {
                Id = 1,
                UserId = 5,
                MenuId = 10
            };

            entity.Id.Should().Be(1);
            entity.UserId.Should().Be(5);
            entity.MenuId.Should().Be(10);
        }

        [Fact]
        public void UserFavoriteMenu_NavigationProperty_Menu_ShouldAcceptNull()
        {
            var entity = new UserFavoriteMenu { Menu = null };

            entity.Menu.Should().BeNull();
        }

        [Fact]
        public void UserFavoriteMenu_NavigationProperty_Menu_ShouldBeAssignable()
        {
            var menu = new Menu { Id = 10, MenuName = "Dashboard" };
            var entity = new UserFavoriteMenu { MenuId = 10, Menu = menu };

            entity.Menu.Should().NotBeNull();
            entity.Menu!.MenuName.Should().Be("Dashboard");
        }
    }
}
