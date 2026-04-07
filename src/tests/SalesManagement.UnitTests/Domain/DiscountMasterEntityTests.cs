using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class DiscountMasterEntityTests
    {
        [Fact]
        public void DiscountMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DiscountMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DiscountMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DiscountMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DiscountMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DiscountMaster)).Should().BeTrue();
        }

        [Fact]
        public void DiscountMaster_Properties_ShouldBeAssignable()
        {
            var entity = new DiscountMaster
            {
                Id = 1,
                DiscountCode = "DISC001",
                DiscountName = "Test Discount",
                DiscountTypeId = 10,
                ApplicableLevelId = 20,
                TriggerEventId = 30,
                RequiresApproval = true,
                MaxDiscountLimitTypeId = 40,
                ValueTypeId = 50,
                DiscountValue = 15.5m,
                SlabTypeId = 60
            };

            entity.Id.Should().Be(1);
            entity.DiscountCode.Should().Be("DISC001");
            entity.DiscountName.Should().Be("Test Discount");
            entity.DiscountTypeId.Should().Be(10);
            entity.ApplicableLevelId.Should().Be(20);
            entity.TriggerEventId.Should().Be(30);
            entity.RequiresApproval.Should().BeTrue();
            entity.MaxDiscountLimitTypeId.Should().Be(40);
            entity.ValueTypeId.Should().Be(50);
            entity.DiscountValue.Should().Be(15.5m);
            entity.SlabTypeId.Should().Be(60);
        }

        [Fact]
        public void DiscountMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DiscountMaster
            {
                MaxDiscountLimitTypeId = null,
                DiscountValue = null,
                SlabTypeId = null
            };

            entity.MaxDiscountLimitTypeId.Should().BeNull();
            entity.DiscountValue.Should().BeNull();
            entity.SlabTypeId.Should().BeNull();
        }

        [Fact]
        public void DiscountMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new DiscountMaster
            {
                DiscountType = new MiscMaster(),
                ApplicableLevel = new MiscMaster(),
                TriggerEvent = new MiscMaster(),
                DiscountSlabs = new List<DiscountSlab>(),
                DiscountSalesGroups = new List<DiscountSalesGroup>(),
                DiscountPaymentTerms = new List<DiscountPaymentTerm>()
            };

            entity.DiscountType.Should().NotBeNull();
            entity.ApplicableLevel.Should().NotBeNull();
            entity.TriggerEvent.Should().NotBeNull();
            entity.DiscountSlabs.Should().NotBeNull();
            entity.DiscountSalesGroups.Should().NotBeNull();
            entity.DiscountPaymentTerms.Should().NotBeNull();
        }
    }
}
