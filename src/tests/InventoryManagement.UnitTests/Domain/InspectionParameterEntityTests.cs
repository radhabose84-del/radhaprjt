using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class InspectionParameterEntityTests
    {
        [Fact]
        public void InspectionParameter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new InspectionParameter();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void InspectionParameter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new InspectionParameter();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void InspectionParameter_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(InspectionParameter)).Should().BeTrue();
        }

        [Fact]
        public void InspectionParameter_Properties_ShouldBeAssignable()
        {
            var entity = new InspectionParameter
            {
                Id = 1,
                TemplateId = 5,
                Parameter = "Visual Inspection",
                Numeric = false
            };

            entity.Id.Should().Be(1);
            entity.TemplateId.Should().Be(5);
            entity.Parameter.Should().Be("Visual Inspection");
            entity.Numeric.Should().BeFalse();
        }

        [Fact]
        public void InspectionParameter_NullableProperties_ShouldAcceptNull()
        {
            var entity = new InspectionParameter
            {
                AcceptanceCriteriaValue = null,
                MinimumValue = null,
                MaximumValue = null
            };

            entity.AcceptanceCriteriaValue.Should().BeNull();
            entity.MinimumValue.Should().BeNull();
            entity.MaximumValue.Should().BeNull();
        }

        [Fact]
        public void InspectionParameter_NumericWithRange_ShouldBeAssignable()
        {
            var entity = new InspectionParameter
            {
                Numeric = true,
                MinimumValue = 0.5m,
                MaximumValue = 10.0m
            };

            entity.Numeric.Should().BeTrue();
            entity.MinimumValue.Should().Be(0.5m);
            entity.MaximumValue.Should().Be(10.0m);
        }
    }
}
