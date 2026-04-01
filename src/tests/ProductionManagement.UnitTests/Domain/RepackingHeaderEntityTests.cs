using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class RepackingHeaderEntityTests
    {
        [Fact]
        public void RepackingHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RepackingHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RepackingHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RepackingHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RepackingHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RepackingHeader)).Should().BeTrue();
        }

        [Fact]
        public void RepackingHeader_DefaultProductionYear_ShouldBeCurrentYear()
        {
            var entity = new RepackingHeader();
            entity.ProductionYear.Should().Be(DateTime.Now.Year);
        }

        [Fact]
        public void RepackingHeader_Properties_ShouldBeAssignable()
        {
            var repackDate = new DateOnly(2025, 7, 10);
            var entity = new RepackingHeader
            {
                Id = 1,
                UnitId = 1,
                ProductionYear = 2025,
                RepackingNo = "RPK-2025-001",
                RepackingDate = repackDate,
                TotalBags = 80,
                NetWeight = 4000.00m,
                LooseConeKgs = 25.00m,
                OldPackHeaderId = 5
            };
            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(1);
            entity.ProductionYear.Should().Be(2025);
            entity.RepackingNo.Should().Be("RPK-2025-001");
            entity.RepackingDate.Should().Be(repackDate);
            entity.TotalBags.Should().Be(80);
            entity.NetWeight.Should().Be(4000.00m);
            entity.LooseConeKgs.Should().Be(25.00m);
            entity.OldPackHeaderId.Should().Be(5);
        }

        [Fact]
        public void RepackingHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RepackingHeader
            {
                RepackingNo = null,
                LooseHandlingId = null,
                Remarks = null
            };
            entity.RepackingNo.Should().BeNull();
            entity.LooseHandlingId.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void RepackingHeader_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new RepackingHeader
            {
                OldPackHeader = null,
                LooseHandling = null,
                RepackingDetails = null
            };
            entity.OldPackHeader.Should().BeNull();
            entity.LooseHandling.Should().BeNull();
            entity.RepackingDetails.Should().BeNull();
        }

        [Fact]
        public void RepackingHeader_NavigationProperties_ShouldBeAssignable()
        {
            var oldPackHeader = new ProductionPackHeader { Id = 5, PackNo = "PKG-2025-005" };
            var looseHandling = new MiscMaster { Id = 3, Description = "Loose Cone Handling" };
            var details = new List<RepackingDetail> { new RepackingDetail { Id = 1 } };

            var entity = new RepackingHeader
            {
                OldPackHeader = oldPackHeader,
                LooseHandling = looseHandling,
                RepackingDetails = details
            };

            entity.OldPackHeader.Should().BeSameAs(oldPackHeader);
            entity.LooseHandling.Should().BeSameAs(looseHandling);
            entity.RepackingDetails.Should().HaveCount(1);
        }
    }
}
