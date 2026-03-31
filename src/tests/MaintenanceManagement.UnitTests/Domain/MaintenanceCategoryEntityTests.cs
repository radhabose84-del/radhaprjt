using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MaintenanceCategoryEntityTests
    {
        [Fact]
        public void MaintenanceCategory_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceCategory();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MaintenanceCategory_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceCategory();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MaintenanceCategory_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MaintenanceCategory)).Should().BeTrue();
        }

        [Fact]
        public void MaintenanceCategory_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceCategory
            {
                Id = 1,
                CategoryName = "Electrical",
                Description = "Electrical maintenance"
            };

            entity.Id.Should().Be(1);
            entity.CategoryName.Should().Be("Electrical");
            entity.Description.Should().Be("Electrical maintenance");
        }

        [Fact]
        public void MaintenanceCategory_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MaintenanceCategory
            {
                CategoryName = null,
                Description = null
            };

            entity.CategoryName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
