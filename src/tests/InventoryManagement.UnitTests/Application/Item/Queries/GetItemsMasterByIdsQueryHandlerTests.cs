using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByIds;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetItemsMasterByIdsQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetItemsMasterByIdsQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyIds_ReturnsEmptyWithoutCallingRepo()
        {
            var result = await CreateSut().Handle(
                new GetItemsMasterByIdsQuery(new List<int>()), CancellationToken.None);

            result.Should().BeEmpty();
            _mockRepo.Verify(r => r.GetItemsMasterByIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullIds_ReturnsEmptyWithoutCallingRepo()
        {
            var result = await CreateSut().Handle(
                new GetItemsMasterByIdsQuery(null!), CancellationToken.None);

            result.Should().BeEmpty();
            _mockRepo.Verify(r => r.GetItemsMasterByIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidIds_ReturnsItems()
        {
            var items = new List<GetItemAutoCompleteDto> { new GetItemAutoCompleteDto { Id = 1 } };
            _mockRepo.Setup(r => r.GetItemsMasterByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(items);

            var result = await CreateSut().Handle(
                new GetItemsMasterByIdsQuery(new List<int> { 1 }), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ValidIds_CallsRepoOnce()
        {
            var items = new List<GetItemAutoCompleteDto>();
            _mockRepo.Setup(r => r.GetItemsMasterByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(items);

            await CreateSut().Handle(
                new GetItemsMasterByIdsQuery(new List<int> { 1, 2, 3 }), CancellationToken.None);

            _mockRepo.Verify(r => r.GetItemsMasterByIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Once);
        }
    }
}
