using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class RepackingDetailEntityTests
    {
        [Fact]
        public void RepackingDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RepackingDetail)).Should().BeFalse();
        }

        [Fact]
        public void RepackingDetail_Properties_ShouldBeAssignable()
        {
            var entity = new RepackingDetail
            {
                Id = 1,
                RepackHeaderId = 10,
                StartPackNo = 1,
                EndPackNo = 20,
                OldStartPackNo = 1,
                OldEndPackNo = 15
            };
            entity.Id.Should().Be(1);
            entity.RepackHeaderId.Should().Be(10);
            entity.StartPackNo.Should().Be(1);
            entity.EndPackNo.Should().Be(20);
            entity.OldStartPackNo.Should().Be(1);
            entity.OldEndPackNo.Should().Be(15);
        }

        [Fact]
        public void RepackingDetail_DefaultValues_ShouldBeDefaults()
        {
            var entity = new RepackingDetail();
            entity.Id.Should().Be(0);
            entity.RepackHeaderId.Should().Be(0);
            entity.StartPackNo.Should().Be(0);
            entity.EndPackNo.Should().Be(0);
            entity.OldStartPackNo.Should().Be(0);
            entity.OldEndPackNo.Should().Be(0);
        }

        [Fact]
        public void RepackingDetail_NavigationProperty_ShouldBeAssignable()
        {
            var header = new RepackingHeader { Id = 5 };
            var entity = new RepackingDetail
            {
                RepackHeaderId = 5,
                RepackingHeader = header
            };
            entity.RepackingHeader.Should().BeSameAs(header);
        }
    }
}
