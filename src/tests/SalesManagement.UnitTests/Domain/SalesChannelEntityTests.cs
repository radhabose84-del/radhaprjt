using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class SalesChannelEntityTests
    {
        [Fact]
        public void SalesChannel_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SalesChannel();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SalesChannel_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesChannel();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SalesChannel_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesChannel)).Should().BeTrue();
        }

        [Fact]
        public void SalesChannel_Properties_ShouldBeAssignable()
        {
            var entity = new SalesChannel
            {
                Id = 2,
                SalesChannelCode = "CH001",
                SalesChannelName = "Online Channel"
            };

            entity.Id.Should().Be(2);
            entity.SalesChannelCode.Should().Be("CH001");
            entity.SalesChannelName.Should().Be("Online Channel");
        }

        [Fact]
        public void SalesChannel_Collection_ShouldBeAssignable()
        {
            var entity = new SalesChannel
            {
                SalesSegments = new List<SalesSegment>()
            };

            entity.SalesSegments.Should().NotBeNull();
        }
    }
}
