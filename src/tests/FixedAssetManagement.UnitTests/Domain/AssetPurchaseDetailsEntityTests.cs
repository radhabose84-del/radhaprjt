using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetPurchaseDetailsEntityTests
    {
        [Fact]
        public void AssetPurchaseDetails_Properties_ShouldBeAssignable()
        {
            var entity = new AssetPurchaseDetails
            {
                Id = 1,
                AssetId = 10,
                AssetSourceId = 2,
                ItemName = "Test Item",
                ItemCode = "ITM001",
                PurchaseValue = 50000m,
                GrnValue = 50000m,
                AcceptedQty = 1,
                GrnNo = 1001,
                GrnSno = 1,
                PoNo = 2001,
                PoSno = 1
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.AssetSourceId.Should().Be(2);
            entity.ItemName.Should().Be("Test Item");
            entity.ItemCode.Should().Be("ITM001");
            entity.PurchaseValue.Should().Be(50000m);
        }

        [Fact]
        public void AssetPurchaseDetails_NullableProperties_AcceptNull()
        {
            var entity = new AssetPurchaseDetails
            {
                BudgetType = null,
                OldUnitId = null,
                VendorCode = null,
                VendorName = null,
                BillNo = null,
                CapitalizationDate = null
            };

            entity.BudgetType.Should().BeNull();
            entity.CapitalizationDate.Should().BeNull();
        }

        [Fact]
        public void AssetPurchaseDetails_DoesNotExtendBaseEntity()
        {
            // AssetPurchaseDetails is a standalone entity (no BaseEntity inheritance)
            typeof(FAM.Domain.Common.BaseEntity).IsAssignableFrom(typeof(AssetPurchaseDetails))
                .Should().BeFalse();
        }

        [Fact]
        public void AssetPurchaseDetails_DefaultValues_AreCorrect()
        {
            var entity = new AssetPurchaseDetails();

            entity.Id.Should().Be(0);
            entity.PurchaseValue.Should().Be(0m);
            entity.GrnValue.Should().Be(0m);
            entity.AcceptedQty.Should().Be(0m);
        }
    }
}
