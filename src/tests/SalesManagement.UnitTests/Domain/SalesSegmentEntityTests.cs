using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class SalesSegmentEntityTests
    {
        [Fact]
        public void SalesSegment_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SalesSegment();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SalesSegment_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesSegment();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SalesSegment_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesSegment)).Should().BeTrue();
        }

        [Fact]
        public void SalesSegment_Properties_ShouldBeAssignable()
        {
            var entity = new SalesSegment
            {
                Id = 4,
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3,
                CurrencyId = 5,
                SegmentName = "Domestic Segment",
                ValidFrom = new DateTime(2025, 1, 1)
            };

            entity.Id.Should().Be(4);
            entity.SalesOrganisationId.Should().Be(1);
            entity.SalesChannelId.Should().Be(2);
            entity.BusinessUnitId.Should().Be(3);
            entity.CurrencyId.Should().Be(5);
            entity.SegmentName.Should().Be("Domestic Segment");
            entity.ValidFrom.Should().Be(new DateTime(2025, 1, 1));
        }

        [Fact]
        public void SalesSegment_NullableCurrencyId_ShouldAcceptNull()
        {
            var entity = new SalesSegment { CurrencyId = null };
            entity.CurrencyId.Should().BeNull();
        }

        [Fact]
        public void SalesSegment_NullableValidFrom_ShouldAcceptNull()
        {
            var entity = new SalesSegment { ValidFrom = null };
            entity.ValidFrom.Should().BeNull();
        }

        [Fact]
        public void SalesSegment_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new SalesSegment
            {
                SalesOrganisation = new SalesOrganisation(),
                SalesChannel = new SalesChannel(),
                BusinessUnit = new BusinessUnit()
            };

            entity.SalesOrganisation.Should().NotBeNull();
            entity.SalesChannel.Should().NotBeNull();
            entity.BusinessUnit.Should().NotBeNull();
        }
    }
}
