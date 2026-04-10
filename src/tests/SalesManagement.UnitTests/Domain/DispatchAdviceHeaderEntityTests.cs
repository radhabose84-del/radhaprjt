using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class DispatchAdviceHeaderEntityTests
    {
        [Fact]
        public void DispatchAdviceHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DispatchAdviceHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DispatchAdviceHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DispatchAdviceHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DispatchAdviceHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DispatchAdviceHeader)).Should().BeTrue();
        }

        [Fact]
        public void DispatchAdviceHeader_Properties_ShouldBeAssignable()
        {
            var entity = new DispatchAdviceHeader
            {
                Id = 1,
                DispatchNo = "DA001",
                DispatchDate = new DateOnly(2026, 1, 1),
                StatusId = 2,
                SalesOrderId = 3,
                PartyId = 4,
                TotOrderQty = 100m,
                TotDispatchedQty = 50m,
                TotPendingQty = 50m,
                DispatchAddressId = 5,
                TransporterId = 6,
                VehicleNo = "KA01AB1234",
                DriverName = "John",
                LRNo = "LR001",
                UnitId = 7,
                InvFlg = false
            };

            entity.Id.Should().Be(1);
            entity.DispatchNo.Should().Be("DA001");
            entity.SalesOrderId.Should().Be(3);
            entity.PartyId.Should().Be(4);
            entity.TotOrderQty.Should().Be(100m);
            entity.InvFlg.Should().BeFalse();
        }

        [Fact]
        public void DispatchAdviceHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new DispatchAdviceHeader
            {
                DispatchAdviceDetails = new List<DispatchAdviceDetail>(),
                InvoiceHeaders = new List<InvoiceHeader>()
            };

            entity.DispatchAdviceDetails.Should().NotBeNull();
            entity.InvoiceHeaders.Should().NotBeNull();
        }
    }
}
