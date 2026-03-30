using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class HSNMasterEntityTests
    {
        [Fact]
        public void HSNMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new HSNMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void HSNMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new HSNMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void HSNMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(HSNMaster)).Should().BeTrue();
        }

        [Fact]
        public void HSNMaster_Properties_ShouldBeAssignable()
        {
            var validFrom = DateTimeOffset.UtcNow;
            var entity = new HSNMaster
            {
                Id = 1,
                HSNCode = "1001",
                Description = "Test Description",
                TypeId = 2,
                GSTCategoryId = 3,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = validFrom
            };

            entity.Id.Should().Be(1);
            entity.HSNCode.Should().Be("1001");
            entity.Description.Should().Be("Test Description");
            entity.TypeId.Should().Be(2);
            entity.GSTCategoryId.Should().Be(3);
            entity.GSTPercentage.Should().Be(18m);
            entity.IGSTPercentage.Should().Be(18m);
            entity.ValidFrom.Should().Be(validFrom);
        }

        [Fact]
        public void HSNMaster_GSTPercentage_SetterAutoCalculatesCGSTAndSGST()
        {
            var entity = new HSNMaster { GSTPercentage = 18m };

            entity.CGSTPercentage.Should().Be(9m);
            entity.SGSTPercentage.Should().Be(9m);
        }

        [Fact]
        public void HSNMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new HSNMaster
            {
                HSNCode = null,
                Description = null,
                Type = null,
                GstCategory = null,
                ItemMasterHSN = null
            };

            entity.HSNCode.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
