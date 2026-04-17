using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
using InventoryManagement.Infrastructure.Repositories.Lookups;

namespace InventoryManagement.IntegrationTests.Repositories.Lookups
{
    /// <summary>
    /// PutawayRuleLookupRepository is a thin adapter that delegates to IPutAwayRuleQueryRepository.
    /// Tests mock the underlying repository and verify mapping + short-circuit behaviour.
    /// </summary>
    public sealed class PutawayRuleLookupRepositoryTests
    {
        private readonly Mock<IPutAwayRuleQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private PutawayRuleLookupRepository CreateRepo() =>
            new(_mockRepo.Object);

        private static GetPutAwayRuleItemIdDto BuildRule(int itemId = 1, int warehouseId = 1) =>
            new()
            {
                PutAwayRuleId = 10,
                StorageTypeId = 1,
                StorageTypeName = "Rack",
                TargetId = 5,
                TargetCode = "T1",
                TargetName = "Target 1",
                PriorityId = 1,
                PriorityName = "High",
                WarehouseId = warehouseId,
                WarehouseCode = "WH",
                WarehouseName = "Warehouse",
                UnitId = 1,
                ItemId = itemId,
                ItemCode = "ITM",
                ItemName = "Item",
                RuleItemId = 100,
                ItemCategoryId = 1,
                ItemCategoryName = "Cat",
                ItemGroupId = 1,
                ItemGroupName = "Grp",
                StockUomId = 1,
                StockUom = "KG",
                PurchaseUomId = 1,
                PurchaseUom = "KG",
                ConversionRate = 1d
            };

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Short_Circuit_When_ItemIds_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>(), new[] { 1 });

            result.Should().BeEmpty();
            _mockRepo.Verify(
                r => r.GetPutAwayRuleDetailsAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Short_Circuit_When_WarehouseIds_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(new[] { 1 }, Array.Empty<int>());

            result.Should().BeEmpty();
            _mockRepo.Verify(
                r => r.GetPutAwayRuleDetailsAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Map_Repository_Results()
        {
            _mockRepo
                .Setup(r => r.GetPutAwayRuleDetailsAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetPutAwayRuleItemIdDto?> { BuildRule() });

            var result = await CreateRepo().GetByIdsAsync(new[] { 1 }, new[] { 1 });

            result.Should().HaveCount(1);
            result[0].ItemId.Should().Be(1);
            result[0].WarehouseId.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Filter_NonPositive_Ids()
        {
            _mockRepo
                .Setup(r => r.GetPutAwayRuleDetailsAsync(It.Is<List<int>>(l => l.Count == 1 && l[0] == 5), It.Is<List<int>>(l => l.Count == 1 && l[0] == 7)))
                .ReturnsAsync(new List<GetPutAwayRuleItemIdDto?> { BuildRule(itemId: 5, warehouseId: 7) });

            var result = await CreateRepo().GetByIdsAsync(new[] { 5, 0, -1 }, new[] { 7, -3 });

            result.Should().HaveCount(1);
        }

        // --- GetPutAwayRuleDetailsAsync ---

        [Fact]
        public async Task GetPutAwayRuleDetailsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetPutAwayRuleDetailsAsync(new List<int>(), new List<int> { 1 });
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPutAwayRuleDetailsAsync_Should_Map_Repository_Results()
        {
            _mockRepo
                .Setup(r => r.GetPutAwayRuleDetailsAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetPutAwayRuleItemIdDto?> { BuildRule() });

            var result = await CreateRepo().GetPutAwayRuleDetailsAsync(new List<int> { 1 }, new List<int> { 1 });

            result.Should().HaveCount(1);
            result[0].PutAwayRuleId.Should().Be(10);
        }

        // --- GetPutAwayRuleDetailsByWarehouseAsync ---

        [Fact]
        public async Task GetPutAwayRuleDetailsByWarehouseAsync_Should_Use_Warehouse_Repo_Method()
        {
            _mockRepo
                .Setup(r => r.GetPutAwayRuleWarehouseDetailsAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetPutAwayRuleItemIdDto?> { BuildRule() });

            var result = await CreateRepo().GetPutAwayRuleDetailsByWarehouseAsync(new List<int> { 1 }, new List<int> { 1 });

            result.Should().HaveCount(1);
            _mockRepo.Verify(
                r => r.GetPutAwayRuleWarehouseDetailsAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPutAwayRuleDetailsByWarehouseAsync_Should_Short_Circuit_For_Empty_Ids()
        {
            var result = await CreateRepo().GetPutAwayRuleDetailsByWarehouseAsync(new List<int>(), new List<int>());
            result.Should().BeEmpty();
        }
    }
}
