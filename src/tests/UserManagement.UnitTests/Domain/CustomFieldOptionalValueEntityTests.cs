using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class CustomFieldOptionalValueEntityTests
    {
        [Fact]
        public void CustomFieldOptionalValue_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CustomFieldOptionalValue)).Should().BeFalse();
        }

        [Fact]
        public void CustomFieldOptionalValue_Properties_ShouldBeAssignable()
        {
            var entity = new CustomFieldOptionalValue
            {
                Id = 1,
                CustomFieldId = 10,
                OptionFieldValue = "Option A"
            };

            entity.Id.Should().Be(1);
            entity.CustomFieldId.Should().Be(10);
            entity.OptionFieldValue.Should().Be("Option A");
        }

        [Fact]
        public void CustomFieldOptionalValue_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CustomFieldOptionalValue
            {
                CustomField = null,
                OptionFieldValue = null
            };

            entity.CustomField.Should().BeNull();
            entity.OptionFieldValue.Should().BeNull();
        }

        [Fact]
        public void CustomFieldOptionalValue_NavigationProperty_CustomField_ShouldBeAssignable()
        {
            var customField = new CustomField { Id = 7 };
            var entity = new CustomFieldOptionalValue { CustomField = customField, CustomFieldId = 7 };

            entity.CustomField.Should().NotBeNull();
            entity.CustomField!.Id.Should().Be(7);
        }
    }
}
