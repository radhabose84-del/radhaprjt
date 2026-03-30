namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetAdditionalCostEntityTests
    {
        [Fact]
        public void AssetAdditionalCost_Properties_ShouldBeAssignable()
        {
            var entity = new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                Id = 1,
                AssetId = 10,
                AssetSourceId = 2,
                Amount = 5000m,
                JournalNo = "JNL001",
                CostType = 1
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.AssetSourceId.Should().Be(2);
            entity.Amount.Should().Be(5000m);
            entity.JournalNo.Should().Be("JNL001");
            entity.CostType.Should().Be(1);
        }

        [Fact]
        public void AssetAdditionalCost_NullableProperties_AcceptNull()
        {
            var entity = new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                JournalNo = null,
                CostType = null
            };

            entity.JournalNo.Should().BeNull();
            entity.CostType.Should().BeNull();
        }

        [Fact]
        public void AssetAdditionalCost_DefaultValues_AreCorrect()
        {
            var entity = new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost();

            entity.Amount.Should().Be(0m);
            entity.AssetId.Should().Be(0);
        }
    }
}
