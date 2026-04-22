using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class SalesTypeEntityTests
    {
        [Fact]
        public void SalesType_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(SalesType)).Should().BeFalse();
        }

        [Fact]
        public void SalesType_Properties_ShouldBeAssignable()
        {
            var entity = new SalesType
            {
                Id = 1,
                PartyId = 10,
                SalesSegmentId = 2,
                OrderTypeId = 3,
                IncotermId = 4,
                PaymentTermsId = 5,
                ShippingConditionId = 6,
                AccountAssignmentId = 7,
                Active = 1
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.SalesSegmentId.Should().Be(2);
            entity.OrderTypeId.Should().Be(3);
            entity.IncotermId.Should().Be(4);
            entity.PaymentTermsId.Should().Be(5);
            entity.ShippingConditionId.Should().Be(6);
            entity.AccountAssignmentId.Should().Be(7);
            entity.Active.Should().Be(1);
        }

        [Fact]
        public void SalesType_NullableProperties_ShouldAcceptNull()
        {
            var entity = new SalesType
            {
                SalesSegmentId = null,
                OrderTypeId = null,
                IncotermId = null,
                PaymentTermsId = null,
                ShippingConditionId = null,
                AccountAssignmentId = null
            };

            entity.SalesSegmentId.Should().BeNull();
            entity.OrderTypeId.Should().BeNull();
            entity.IncotermId.Should().BeNull();
            entity.PaymentTermsId.Should().BeNull();
            entity.ShippingConditionId.Should().BeNull();
            entity.AccountAssignmentId.Should().BeNull();
        }

        [Fact]
        public void SalesType_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10 };
            var shippingMisc = new MiscMaster { Id = 6 };
            var accountMisc = new MiscMaster { Id = 7 };

            var entity = new SalesType
            {
                Party = party,
                ShippingConditionMisc = shippingMisc,
                AccountAssignmentMisc = accountMisc
            };

            entity.Party.Should().NotBeNull();
            entity.ShippingConditionMisc.Should().NotBeNull();
            entity.AccountAssignmentMisc.Should().NotBeNull();
        }
    }
}
