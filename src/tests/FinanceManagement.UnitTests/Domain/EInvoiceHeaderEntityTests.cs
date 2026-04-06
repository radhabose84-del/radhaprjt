using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class EInvoiceHeaderEntityTests
    {
        [Fact]
        public void EInvoiceHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new EInvoiceHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void EInvoiceHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new EInvoiceHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void EInvoiceHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(EInvoiceHeader)).Should().BeTrue();
        }

        [Fact]
        public void EInvoiceHeader_Properties_ShouldBeAssignable()
        {
            var entity = new EInvoiceHeader
            {
                Id = 1,
                UnitId = 10,
                DocType = "INV",
                SupplyType = "B2B",
                InvoiceNo = "INV001",
                InvoiceDate = new DateOnly(2026, 1, 15),
                PlaceOfSupply = "29",
                IrnNumber = "IRN123456",
                AckNo = "ACK001",
                AckDate = DateTimeOffset.UtcNow,
                SignInvoice = "signdata",
                SignQrCode = "qrdata",
                IrnStatus = "Generated",
                ErrorCode = null,
                ErrorMessage = null,
                PartyId = 5,
                GstNo = "29AAAAA1234A1Z5",
                ReverseCharge = false,
                CGST = 100.50m,
                SGST = 100.50m,
                IGST = 0m,
                Cess = 0m,
                StateCess = 0m,
                TCS = 0m,
                Discount = 50m,
                OtherCharges = 10m,
                RoundOff = 0.50m,
                InvoiceAmount = 10000m,
                Remarks = "Test",
                StatusId = 1,
                EWaybillCreated = true
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.DocType.Should().Be("INV");
            entity.SupplyType.Should().Be("B2B");
            entity.InvoiceNo.Should().Be("INV001");
            entity.InvoiceDate.Should().Be(new DateOnly(2026, 1, 15));
            entity.PartyId.Should().Be(5);
            entity.CGST.Should().Be(100.50m);
            entity.InvoiceAmount.Should().Be(10000m);
            entity.EWaybillCreated.Should().BeTrue();
        }

        [Fact]
        public void EInvoiceHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new EInvoiceHeader
            {
                DocType = null,
                SupplyType = null,
                InvoiceNo = null,
                IrnNumber = null,
                AckNo = null,
                AckDate = null,
                SignInvoice = null,
                SignQrCode = null,
                IrnStatus = null,
                ErrorCode = null,
                ErrorMessage = null,
                GstNo = null,
                Remarks = null,
                StatusId = null
            };

            entity.DocType.Should().BeNull();
            entity.IrnNumber.Should().BeNull();
            entity.AckDate.Should().BeNull();
            entity.StatusId.Should().BeNull();
        }

        [Fact]
        public void EInvoiceHeader_Collections_ShouldBeAssignable()
        {
            var entity = new EInvoiceHeader
            {
                EInvoiceDetails = new List<EInvoiceDetail> { new EInvoiceDetail { Id = 1 } },
                EWaybillHeaders = new List<EWaybillHeader> { new EWaybillHeader { Id = 1 } }
            };

            entity.EInvoiceDetails.Should().HaveCount(1);
            entity.EWaybillHeaders.Should().HaveCount(1);
        }

        [Fact]
        public void EInvoiceHeader_Collections_ShouldAcceptNull()
        {
            var entity = new EInvoiceHeader
            {
                EInvoiceDetails = null,
                EWaybillHeaders = null
            };

            entity.EInvoiceDetails.Should().BeNull();
            entity.EWaybillHeaders.Should().BeNull();
        }
    }
}
