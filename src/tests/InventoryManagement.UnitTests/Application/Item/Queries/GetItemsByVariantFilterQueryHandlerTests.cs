using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByVariantFilter;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetItemsByVariantFilterQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemsByVariantFilterQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object);

        private void SetupDefaults(List<GetItemAutoCompleteDto> items)
        {
            _mockRepo.Setup(r => r.GetItemsByVariantFilterAsync(
                    It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var items = new List<GetItemAutoCompleteDto> { new GetItemAutoCompleteDto { Id = 1 } };
            SetupDefaults(items);

            var result = await CreateSut().Handle(new GetItemsByVariantFilterQuery { HasVariant = true }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmpty()
        {
            SetupDefaults(new List<GetItemAutoCompleteDto>());

            var result = await CreateSut().Handle(new GetItemsByVariantFilterQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            SetupDefaults(new List<GetItemAutoCompleteDto>());

            await CreateSut().Handle(new GetItemsByVariantFilterQuery { HasVariant = false, ParentItemId = 1 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetItemsByVariantFilterAsync(
                It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
