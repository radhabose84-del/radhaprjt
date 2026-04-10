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
            var entity = new SalesManagement.Domain.Entities.DiscountMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DiscountMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesManagement.Domain.Entities.DiscountMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DiscountMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesManagement.Domain.Entities.DiscountMaster)).Should().BeTrue();
        }

        [Fact]
        public void DiscountMaster_Properties_ShouldBeAssignable()
        {
            var entity = new SalesManagement.Domain.Entities.DiscountMaster
            {
                Id = 1,
                DiscountCode = "DC001",
                DiscountName = "Test Discount",
                TriggerEventId = 10,
                DiscountBasisId = 20,
                ExecutionTypeId = 30,
                CurrencyId = 40,
                CustomerGroupId = 50,
                Priority = 1,
                RequiresApproval = true,
                MaxDiscountLimitTypeId = 60,
                MaxDiscountValue = 500m,
                IsStackable = false,
                ExclusionGroupId = 70,
                ValueTypeId = 80,
                SlabTypeId = 90
            };

            entity.Id.Should().Be(1);
            entity.DiscountCode.Should().Be("DC001");
            entity.DiscountName.Should().Be("Test Discount");
            entity.TriggerEventId.Should().Be(10);
            entity.DiscountBasisId.Should().Be(20);
            entity.ExecutionTypeId.Should().Be(30);
            entity.CurrencyId.Should().Be(40);
            entity.CustomerGroupId.Should().Be(50);
            entity.Priority.Should().Be(1);
            entity.RequiresApproval.Should().BeTrue();
            entity.MaxDiscountLimitTypeId.Should().Be(60);
            entity.MaxDiscountValue.Should().Be(500m);
            entity.IsStackable.Should().BeFalse();
            entity.ExclusionGroupId.Should().Be(70);
            entity.ValueTypeId.Should().Be(80);
            entity.SlabTypeId.Should().Be(90);
        }

        [Fact]
        public void DiscountMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new SalesManagement.Domain.Entities.DiscountMaster
            {
                CurrencyId = null,
                CustomerGroupId = null,
                MaxDiscountLimitTypeId = null,
                MaxDiscountValue = null,
                ExclusionGroupId = null
            };

            entity.CurrencyId.Should().BeNull();
            entity.CustomerGroupId.Should().BeNull();
            entity.MaxDiscountLimitTypeId.Should().BeNull();
            entity.MaxDiscountValue.Should().BeNull();
            entity.ExclusionGroupId.Should().BeNull();
        }

        [Fact]
        public void DiscountMaster_ChildCollections_ShouldBeAssignable()
        {
            var entity = new SalesManagement.Domain.Entities.DiscountMaster
            {
                DiscountSlabs = new List<DiscountSlab>(),
                DiscountSalesGroups = new List<DiscountSalesGroup>(),
                DiscountPaymentTerms = new List<DiscountPaymentTerm>()
            };

            entity.DiscountSlabs.Should().NotBeNull();
            entity.DiscountSalesGroups.Should().NotBeNull();
            entity.DiscountPaymentTerms.Should().NotBeNull();
        }

        [Fact]
        public void DiscountMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new SalesManagement.Domain.Entities.DiscountMaster
            {
                TriggerEvent = new MiscMaster(),
                DiscountBasis = new MiscMaster(),
                ExecutionType = new MiscMaster()
            };

            entity.TriggerEvent.Should().NotBeNull();
            entity.DiscountBasis.Should().NotBeNull();
            entity.ExecutionType.Should().NotBeNull();
        }
    }
}
