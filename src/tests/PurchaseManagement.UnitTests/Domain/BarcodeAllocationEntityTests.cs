using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class BarcodeAllocationEntityTests
    {
        [Fact]
        public void BarcodeAllocation_DefaultIsActive_ShouldBeActive()
        {
            new BarcodeAllocation().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BarcodeAllocation_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new BarcodeAllocation().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BarcodeAllocation_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BarcodeAllocation)).Should().BeTrue();
        }

        [Fact]
        public void BarcodeAllocation_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new BarcodeAllocation
            {
                Id = 1,
                AllocationNumber = "BBA-2025-0001",
                AllocationDate = now,
                EmployeeNo = "1023",
                EmployeeName = "Rajesh Kumar",
                BarcodeSeriesId = 7,
                BarcodeFrom = 25000001,
                BarcodeTo = 25000500,
                UsedQuantity = 0,
                StatusId = 1202,
                Remarks = "A"
            };

            entity.AllocationNumber.Should().Be("BBA-2025-0001");
            entity.EmployeeNo.Should().Be("1023");
            entity.EmployeeName.Should().Be("Rajesh Kumar");
            entity.BarcodeSeriesId.Should().Be(7);
            entity.BarcodeFrom.Should().Be(25000001);
            entity.BarcodeTo.Should().Be(25000500);
            entity.StatusId.Should().Be(1202);
        }

        [Fact]
        public void BarcodeAllocation_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BarcodeAllocation
            {
                AllocationNumber = null,
                EmployeeNo = null,
                EmployeeName = null,
                Remarks = null,
                BarcodeSeries = null,
                Status = null
            };

            entity.AllocationNumber.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.BarcodeSeries.Should().BeNull();
        }

        [Fact]
        public void BarcodeAllocation_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new BarcodeAllocation
            {
                BarcodeSeries = new BarcodeSeries { Id = 1, BarcodeSeriesNumber = "BCS-2025-0001" },
                Status = new MiscMaster { Id = 1202, Code = "Open" }
            };

            entity.BarcodeSeries!.BarcodeSeriesNumber.Should().Be("BCS-2025-0001");
            entity.Status!.Code.Should().Be("Open");
        }
    }
}
