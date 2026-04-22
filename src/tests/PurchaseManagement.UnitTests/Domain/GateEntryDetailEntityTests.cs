using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Domain
{
    public class GateEntryDetailEntityTests
    {
        [Fact]
        public void GateEntryDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GateEntryDetail)).Should().BeFalse();
        }

        [Fact]
        public void GateEntryDetail_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new GateEntryDetail
            {
                Id = 1,
                GateEntryHeaderId = 10,
                PoCategoryId = 2,
                PoMethodId = 3,
                PoId = 100,
                PoDate = now,
                PoCreatedBy = "admin",
                GSTNumber = "22AAAAA1234A1Z5",
                ContactDetails = "9876543210"
            };

            entity.Id.Should().Be(1);
            entity.GateEntryHeaderId.Should().Be(10);
            entity.PoCategoryId.Should().Be(2);
            entity.PoMethodId.Should().Be(3);
            entity.PoId.Should().Be(100);
            entity.PoDate.Should().Be(now);
            entity.PoCreatedBy.Should().Be("admin");
            entity.GSTNumber.Should().Be("22AAAAA1234A1Z5");
            entity.ContactDetails.Should().Be("9876543210");
        }

        [Fact]
        public void GateEntryDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GateEntryDetail
            {
                GSTNumber = null,
                ContactDetails = null,
                PoType = null,
                PoGateMethodDetails = null
            };

            entity.GSTNumber.Should().BeNull();
            entity.ContactDetails.Should().BeNull();
            entity.PoType.Should().BeNull();
            entity.PoGateMethodDetails.Should().BeNull();
        }

        [Fact]
        public void GateEntryDetail_NavigationProperties_ShouldBeAssignable()
        {
            var header = new GateEntryHeader();
            var po = new PurchaseOrderHeader();
            var entity = new GateEntryDetail
            {
                GateEntryHeaderDetails = header,
                GatePurchaseOrder = po
            };

            entity.GateEntryHeaderDetails.Should().BeSameAs(header);
            entity.GatePurchaseOrder.Should().BeSameAs(po);
        }
    }
}
