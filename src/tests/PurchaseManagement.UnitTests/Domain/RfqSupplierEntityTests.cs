using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.ValueObjects;

namespace PurchaseManagement.UnitTests.Domain
{
    public class RfqSupplierEntityTests
    {
        [Fact]
        public void RfqSupplier_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RfqSupplier)).Should().BeFalse();
        }

        [Fact]
        public void RfqSupplier_ShouldImplementIActivityTracked()
        {
            typeof(IActivityTracked).IsAssignableFrom(typeof(RfqSupplier)).Should().BeTrue();
        }

        [Fact]
        public void RfqSupplier_Properties_ShouldBeAssignable()
        {
            var email = new EmailAddress("supplier@example.com");
            var entity = new RfqSupplier
            {
                Id = 1,
                RfqId = 10,
                SupplierId = 20,
                Name = "Test Supplier",
                Email = email,
                Mobile = "9876543210",
                GSTNumber = "22AAAAA1234A1Z5"
            };

            entity.Id.Should().Be(1);
            entity.RfqId.Should().Be(10);
            entity.SupplierId.Should().Be(20);
            entity.Name.Should().Be("Test Supplier");
            entity.Email.Should().BeSameAs(email);
            entity.Mobile.Should().Be("9876543210");
            entity.GSTNumber.Should().Be("22AAAAA1234A1Z5");
        }

        [Fact]
        public void RfqSupplier_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RfqSupplier
            {
                SupplierId = null,
                Name = null,
                Email = null,
                Mobile = null,
                GSTNumber = null
            };

            entity.SupplierId.Should().BeNull();
            entity.Name.Should().BeNull();
            entity.Email.Should().BeNull();
            entity.Mobile.Should().BeNull();
            entity.GSTNumber.Should().BeNull();
        }

        [Fact]
        public void RfqSupplier_NavigationProperty_ShouldBeAssignable()
        {
            var rfq = new RfqMaster();
            var entity = new RfqSupplier
            {
                Rfq = rfq
            };

            entity.Rfq.Should().BeSameAs(rfq);
        }
    }
}
