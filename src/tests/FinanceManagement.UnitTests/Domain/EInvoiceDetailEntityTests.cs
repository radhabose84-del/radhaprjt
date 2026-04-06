using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class EInvoiceDetailEntityTests
    {
        [Fact]
        public void EInvoiceDetail_Properties_ShouldBeAssignable()
        {
            var entity = new EInvoiceDetail
            {
                Id = 1,
                EInvoiceHeaderId = 10,
                ItemSno = 1,
                ItemId = 100,
                ItemName = "Test Item",
                HsnNo = "1234",
                NoOfBags = 5,
                Qty = 100.5m,
                UnitPrice = 50.25m,
                Rate = 50.00m,
                Discount = 10m,
                TaxableAmount = 5000m,
                GstPercentage = 18m,
                CGST = 450m,
                SGST = 450m,
                IGST = 0m,
                OtherCharges = 5m,
                TotalAmount = 5905m,
                IsService = "N",
                GrossAmount = 5025m,
                FreeQty = 0m,
                CessRate = 0m,
                CessAmount = 0m,
                PackTypeId = 2,
                UOM = "KGS"
            };

            entity.Id.Should().Be(1);
            entity.EInvoiceHeaderId.Should().Be(10);
            entity.ItemSno.Should().Be(1);
            entity.ItemId.Should().Be(100);
            entity.ItemName.Should().Be("Test Item");
            entity.HsnNo.Should().Be("1234");
            entity.NoOfBags.Should().Be(5);
            entity.Qty.Should().Be(100.5m);
            entity.UnitPrice.Should().Be(50.25m);
            entity.TaxableAmount.Should().Be(5000m);
            entity.GstPercentage.Should().Be(18m);
            entity.CGST.Should().Be(450m);
            entity.TotalAmount.Should().Be(5905m);
            entity.IsService.Should().Be("N");
            entity.UOM.Should().Be("KGS");
        }

        [Fact]
        public void EInvoiceDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new EInvoiceDetail
            {
                ItemName = null,
                HsnNo = null,
                IsService = null,
                PackTypeId = null,
                UOM = null
            };

            entity.ItemName.Should().BeNull();
            entity.HsnNo.Should().BeNull();
            entity.IsService.Should().BeNull();
            entity.PackTypeId.Should().BeNull();
            entity.UOM.Should().BeNull();
        }

        [Fact]
        public void EInvoiceDetail_NavigationProperty_ShouldBeAssignable()
        {
            var header = new EInvoiceHeader { Id = 10 };
            var entity = new EInvoiceDetail { EInvoiceHeader = header };

            entity.EInvoiceHeader.Should().NotBeNull();
            entity.EInvoiceHeader!.Id.Should().Be(10);
        }

        [Fact]
        public void EInvoiceDetail_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new EInvoiceDetail { EInvoiceHeader = null };
            entity.EInvoiceHeader.Should().BeNull();
        }
    }
}
