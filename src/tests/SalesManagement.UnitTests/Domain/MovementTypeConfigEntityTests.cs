using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class MovementTypeConfigEntityTests
    {
        [Fact]
        public void MovementTypeConfig_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MovementTypeConfig();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MovementTypeConfig_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MovementTypeConfig();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MovementTypeConfig_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MovementTypeConfig)).Should().BeTrue();
        }

        [Fact]
        public void MovementTypeConfig_Properties_ShouldBeAssignable()
        {
            var entity = new MovementTypeConfig
            {
                Id = 1,
                MovementCode = "MOVE01",
                MovementDescription = "Test Movement",
                MovementCategoryId = 10,
                FromStockTypeId = 20,
                ToStockTypeId = 30,
                QuantityUpdateFlag = true,
                ValueUpdateFlag = false,
                AccountModifier = "ACC001",
                BatchRequiredFlag = true,
                NegativeStockAllowed = false
            };

            entity.Id.Should().Be(1);
            entity.MovementCode.Should().Be("MOVE01");
            entity.MovementDescription.Should().Be("Test Movement");
            entity.MovementCategoryId.Should().Be(10);
            entity.FromStockTypeId.Should().Be(20);
            entity.ToStockTypeId.Should().Be(30);
            entity.QuantityUpdateFlag.Should().BeTrue();
            entity.ValueUpdateFlag.Should().BeFalse();
            entity.AccountModifier.Should().Be("ACC001");
            entity.BatchRequiredFlag.Should().BeTrue();
            entity.NegativeStockAllowed.Should().BeFalse();
        }

        [Fact]
        public void MovementTypeConfig_DefaultQuantityUpdateFlag_ShouldBeTrue()
        {
            var entity = new MovementTypeConfig();
            entity.QuantityUpdateFlag.Should().BeTrue();
        }

        [Fact]
        public void MovementTypeConfig_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new MovementTypeConfig
            {
                MovementCategory = new MiscMaster(),
                FromStockType = new MiscMaster(),
                ToStockType = new MiscMaster(),
                StoTypeMastersAsPgi = new List<StoTypeMaster>(),
                StoTypeMastersAsGr = new List<StoTypeMaster>()
            };

            entity.MovementCategory.Should().NotBeNull();
            entity.FromStockType.Should().NotBeNull();
            entity.ToStockType.Should().NotBeNull();
            entity.StoTypeMastersAsPgi.Should().NotBeNull();
            entity.StoTypeMastersAsGr.Should().NotBeNull();
        }
    }
}
