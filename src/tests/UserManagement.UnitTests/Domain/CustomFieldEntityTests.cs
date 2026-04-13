using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class CustomFieldEntityTests
    {
        [Fact]
        public void CustomField_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new CustomField();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void CustomField_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CustomField();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CustomField_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CustomField)).Should().BeTrue();
        }

        [Fact]
        public void CustomField_Properties_ShouldBeAssignable()
        {
            var entity = new CustomField
            {
                Id = 1,
                LabelName = "Custom Label",
                DataTypeId = 5,
                Length = 100,
                LabelTypeId = 7,
                IsRequired = 1
            };

            entity.Id.Should().Be(1);
            entity.LabelName.Should().Be("Custom Label");
            entity.DataTypeId.Should().Be(5);
            entity.Length.Should().Be(100);
            entity.LabelTypeId.Should().Be(7);
            entity.IsRequired.Should().Be((byte)1);
        }

        [Fact]
        public void CustomField_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CustomField
            {
                LabelName = null,
                Length = null,
                DataType = null,
                LabelType = null
            };

            entity.LabelName.Should().BeNull();
            entity.Length.Should().BeNull();
            entity.DataType.Should().BeNull();
            entity.LabelType.Should().BeNull();
        }

        [Fact]
        public void CustomField_NavigationCollections_ShouldDefaultToEmptyList()
        {
            var entity = new CustomField();

            entity.CustomFieldMenu.Should().NotBeNull();
            entity.CustomFieldMenu.Should().BeEmpty();
            entity.CustomFieldUnits.Should().NotBeNull();
            entity.CustomFieldUnits.Should().BeEmpty();
            entity.CustomFieldOptionalValues.Should().NotBeNull();
            entity.CustomFieldOptionalValues.Should().BeEmpty();
        }
    }
}
