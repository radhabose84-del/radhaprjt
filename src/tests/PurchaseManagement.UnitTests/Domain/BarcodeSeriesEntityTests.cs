using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class BarcodeSeriesEntityTests
    {
        [Fact]
        public void BarcodeSeries_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BarcodeSeries();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BarcodeSeries_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BarcodeSeries();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BarcodeSeries_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BarcodeSeries)).Should().BeTrue();
        }

        [Fact]
        public void BarcodeSeries_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new BarcodeSeries
            {
                Id = 1,
                BarcodeSeriesNumber = "BCS-2025-0001",
                PrefixId = 5,
                BarcodeStartNumber = 25000001,
                BarcodeEndNumber = 25005000,
                GenerationDate = now,
                AllocatedCount = 0,
                StatusId = 10,
                Remarks = "Range A"
            };

            entity.Id.Should().Be(1);
            entity.BarcodeSeriesNumber.Should().Be("BCS-2025-0001");
            entity.PrefixId.Should().Be(5);
            entity.BarcodeStartNumber.Should().Be(25000001);
            entity.BarcodeEndNumber.Should().Be(25005000);
            entity.GenerationDate.Should().Be(now);
            entity.StatusId.Should().Be(10);
        }

        [Fact]
        public void BarcodeSeries_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BarcodeSeries
            {
                BarcodeSeriesNumber = null,
                Remarks = null,
                Prefix = null,
                Status = null
            };

            entity.BarcodeSeriesNumber.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.Prefix.Should().BeNull();
            entity.Status.Should().BeNull();
        }

        [Fact]
        public void BarcodeSeries_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new BarcodeSeries
            {
                Prefix = new MiscMaster { Id = 1, Code = "CB" },
                Status = new MiscMaster { Id = 10, Code = "Open" }
            };

            entity.Prefix!.Code.Should().Be("CB");
            entity.Status!.Code.Should().Be("Open");
        }
    }
}
