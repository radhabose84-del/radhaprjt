using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class UOMEntityTests
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
                UOMTypeId = 2,
                SortOrder = 1
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("KG");
            entity.UOMName.Should().Be("Kilogram");
            entity.UOMTypeId.Should().Be(2);
            entity.SortOrder.Should().Be(1);
        }

        [Fact]
        public void UOM_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UOM
            {
                Code = null,
                UOMName = null
            };

            entity.Code.Should().BeNull();
            entity.UOMName.Should().BeNull();
        }
    }
}
