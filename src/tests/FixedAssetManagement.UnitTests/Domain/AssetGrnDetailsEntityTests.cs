using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetGrnDetailsEntityTests
    {
        [Fact]
        public void AssetGrnDetails_Properties_ShouldBeAssignable()
        {
            var poDate = DateTimeOffset.UtcNow;
            var grnDate = DateTimeOffset.UtcNow.AddDays(1);
            var billDate = DateTimeOffset.UtcNow.AddDays(2);

            var entity = new AssetGrnDetails
            {
                BudgetType = "Capital",
                OldUnitId = "U001",
                VendorCode = "V001",
                VendorName = "Vendor A",
                PoDate = poDate,
                PoNo = "PO-001",
                PoSno = "1",
                ItemCode = "ITM001",
                ItemName = "Laptop",
                GrnNo = "GRN-001",
                GrnSno = "1",
                GrnDate = grnDate,
                QcCompleted = 'Y',
                AcceptedQty = 10.5m,
                PurchaseValue = 50000m,
                GrnValue = 48000m,
                BillNo = "BILL-001",
                BillDate = billDate,
                Uom = "NOS",
                BinLocation = "BIN-A1",
                PjYear = "2025",
                PjDocId = "DOC001",
                PjDocSr = "SR001",
                PjDocNo = "DN001"
            };

            entity.BudgetType.Should().Be("Capital");
            entity.OldUnitId.Should().Be("U001");
            entity.VendorCode.Should().Be("V001");
            entity.VendorName.Should().Be("Vendor A");
            entity.PoDate.Should().Be(poDate);
            entity.PoNo.Should().Be("PO-001");
            entity.PoSno.Should().Be("1");
            entity.ItemCode.Should().Be("ITM001");
            entity.ItemName.Should().Be("Laptop");
            entity.GrnNo.Should().Be("GRN-001");
            entity.GrnSno.Should().Be("1");
            entity.GrnDate.Should().Be(grnDate);
            entity.QcCompleted.Should().Be('Y');
            entity.AcceptedQty.Should().Be(10.5m);
            entity.PurchaseValue.Should().Be(50000m);
            entity.GrnValue.Should().Be(48000m);
            entity.BillNo.Should().Be("BILL-001");
            entity.BillDate.Should().Be(billDate);
            entity.Uom.Should().Be("NOS");
            entity.BinLocation.Should().Be("BIN-A1");
            entity.PjYear.Should().Be("2025");
            entity.PjDocId.Should().Be("DOC001");
            entity.PjDocSr.Should().Be("SR001");
            entity.PjDocNo.Should().Be("DN001");
        }

        [Fact]
        public void AssetGrnDetails_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetGrnDetails
            {
                PoDate = DateTimeOffset.UtcNow,
                GrnDate = DateTimeOffset.UtcNow,
                BillDate = DateTimeOffset.UtcNow
            };

            entity.BudgetType.Should().BeNull();
            entity.OldUnitId.Should().BeNull();
            entity.VendorCode.Should().BeNull();
            entity.VendorName.Should().BeNull();
            entity.PoNo.Should().BeNull();
            entity.PoSno.Should().BeNull();
            entity.ItemCode.Should().BeNull();
            entity.ItemName.Should().BeNull();
            entity.GrnNo.Should().BeNull();
            entity.GrnSno.Should().BeNull();
            entity.QcCompleted.Should().BeNull();
            entity.BillNo.Should().BeNull();
            entity.Uom.Should().BeNull();
            entity.BinLocation.Should().BeNull();
            entity.PjYear.Should().BeNull();
            entity.PjDocId.Should().BeNull();
            entity.PjDocSr.Should().BeNull();
            entity.PjDocNo.Should().BeNull();
        }
    }
}
