using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class CustomFieldUnitEntityTests
    {
        [Fact]
        public void CustomFieldUnit_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CustomFieldUnit)).Should().BeFalse();
        }

        [Fact]
        public void CustomFieldUnit_Properties_ShouldBeAssignable()
        {
            var entity = new CustomFieldUnit
            {
                Id = 1,
                CustomFieldId = 10,
                UnitId = 20
            };

            entity.Id.Should().Be(1);
            entity.CustomFieldId.Should().Be(10);
            entity.UnitId.Should().Be(20);
        }

        [Fact]
        public void CustomFieldUnit_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CustomFieldUnit
            {
                CustomField = null,
                Unit = null
            };

            entity.CustomField.Should().BeNull();
            entity.Unit.Should().BeNull();
        }

        [Fact]
        public void CustomFieldUnit_NavigationProperty_CustomField_ShouldBeAssignable()
        {
            var customField = new CustomField { Id = 3 };
            var entity = new CustomFieldUnit { CustomField = customField, CustomFieldId = 3 };

            entity.CustomField.Should().NotBeNull();
            entity.CustomField!.Id.Should().Be(3);
        }

        [Fact]
        public void CustomFieldUnit_NavigationProperty_Unit_ShouldBeAssignable()
        {
            var unit = new Unit { Id = 4 };
            var entity = new CustomFieldUnit { Unit = unit, UnitId = 4 };

            entity.Unit.Should().NotBeNull();
            entity.Unit!.Id.Should().Be(4);
        }
    }
}
