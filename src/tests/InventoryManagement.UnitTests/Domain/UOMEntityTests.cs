using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class UOMEntityTests
    {
        [Fact]
        public void UOM_DefaultIsActive_ShouldBeActive()
        {
            var entity = new UOM();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UOM_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UOM();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UOM_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UOM)).Should().BeTrue();
        }

        [Fact]
        public void UOM_Properties_ShouldBeAssignable()
        {
            var entity = new UOM
            {
                Id = 1,
                Code = "KG",
                UOMName = "Kilogram",
                UOMTypeId = 5,
                SortOrder = 10
            };
            entity.Id.Should().Be(1);
            entity.Code.Should().Be("KG");
            entity.UOMName.Should().Be("Kilogram");
            entity.UOMTypeId.Should().Be(5);
            entity.SortOrder.Should().Be(10);
        }
    }
}
