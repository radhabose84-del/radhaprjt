using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class PriceGroupMasterEntityTests
    {
        [Fact]
        public void PriceGroupMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PriceGroupMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PriceGroupMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PriceGroupMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PriceGroupMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PriceGroupMaster)).Should().BeTrue();
        }

        [Fact]
        public void PriceGroupMaster_Properties_ShouldBeAssignable()
        {
            var from = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var to = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero);

            var entity = new PriceGroupMaster
            {
                Id = 1,
                PriceGroupCode = "PG001",
                PriceGroupName = "Standard",
                Description = "Standard tier",
                EffectiveFrom = from,
                EffectiveTo = to
            };

            entity.Id.Should().Be(1);
            entity.PriceGroupCode.Should().Be("PG001");
            entity.PriceGroupName.Should().Be("Standard");
            entity.Description.Should().Be("Standard tier");
            entity.EffectiveFrom.Should().Be(from);
            entity.EffectiveTo.Should().Be(to);
        }

        [Fact]
        public void PriceGroupMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PriceGroupMaster
            {
                PriceGroupCode = null,
                PriceGroupName = null,
                Description = null,
                EffectiveTo = null
            };

            entity.PriceGroupCode.Should().BeNull();
            entity.PriceGroupName.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.EffectiveTo.Should().BeNull();
        }
    }
}
