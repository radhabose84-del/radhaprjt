using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class CustomFieldMenuEntityTests
    {
        [Fact]
        public void CustomFieldMenu_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CustomFieldMenu)).Should().BeFalse();
        }

        [Fact]
        public void CustomFieldMenu_Properties_ShouldBeAssignable()
        {
            var entity = new CustomFieldMenu
            {
                Id = 1,
                CustomFieldId = 10,
                MenuId = 20
            };

            entity.Id.Should().Be(1);
            entity.CustomFieldId.Should().Be(10);
            entity.MenuId.Should().Be(20);
        }

        [Fact]
        public void CustomFieldMenu_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CustomFieldMenu
            {
                CustomField = null,
                Menu = null
            };

            entity.CustomField.Should().BeNull();
            entity.Menu.Should().BeNull();
        }

        [Fact]
        public void CustomFieldMenu_NavigationProperty_CustomField_ShouldBeAssignable()
        {
            var customField = new CustomField { Id = 5 };
            var entity = new CustomFieldMenu { CustomField = customField, CustomFieldId = 5 };

            entity.CustomField.Should().NotBeNull();
            entity.CustomField!.Id.Should().Be(5);
        }

        [Fact]
        public void CustomFieldMenu_NavigationProperty_Menu_ShouldBeAssignable()
        {
            var menu = new Menu { Id = 8 };
            var entity = new CustomFieldMenu { Menu = menu, MenuId = 8 };

            entity.Menu.Should().NotBeNull();
            entity.Menu!.Id.Should().Be(8);
        }
    }
}
