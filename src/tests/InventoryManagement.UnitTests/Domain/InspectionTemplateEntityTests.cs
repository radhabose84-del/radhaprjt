using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class InspectionTemplateEntityTests
    {
        [Fact]
        public void InspectionTemplate_DefaultIsActive_ShouldBeActive()
        {
            var entity = new InspectionTemplate();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void InspectionTemplate_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new InspectionTemplate();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void InspectionTemplate_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(InspectionTemplate)).Should().BeTrue();
        }

        [Fact]
        public void InspectionTemplate_Properties_ShouldBeAssignable()
        {
            var entity = new InspectionTemplate
            {
                Id = 1,
                TemplateName = "Quality Check Template"
            };

            entity.Id.Should().Be(1);
            entity.TemplateName.Should().Be("Quality Check Template");
        }

        [Fact]
        public void InspectionTemplate_CanBeInstantiated()
        {
            var act = () => new InspectionTemplate();
            act.Should().NotThrow();
        }

        [Fact]
        public void InspectionTemplate_Parameters_DefaultsToEmpty()
        {
            var entity = new InspectionTemplate();
            entity.Parameters.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void InspectionTemplate_Items_DefaultsToEmpty()
        {
            var entity = new InspectionTemplate();
            entity.Items.Should().NotBeNull().And.BeEmpty();
        }
    }
}
