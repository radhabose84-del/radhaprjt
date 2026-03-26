using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class UOMConversionEntityTests
    {
        [Fact]
        public void UOMConversion_DefaultIsActive_ShouldBeActive()
        {
            var entity = new UOMConversion();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UOMConversion_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UOMConversion();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UOMConversion_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UOMConversion)).Should().BeTrue();
        }

        [Fact]
        public void UOMConversion_Properties_ShouldBeAssignable()
        {
            var entity = new UOMConversion
            {
                Id = 1,
                FromUOMId = 10,
                ToUOMId = 20,
                ConversionValue = 1000m
            };

            entity.Id.Should().Be(1);
            entity.FromUOMId.Should().Be(10);
            entity.ToUOMId.Should().Be(20);
            entity.ConversionValue.Should().Be(1000m);
        }

        [Fact]
        public void UOMConversion_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new UOMConversion
            {
                FromUOM = null,
                ToUOM = null
            };

            entity.FromUOM.Should().BeNull();
            entity.ToUOM.Should().BeNull();
        }
    }
}
