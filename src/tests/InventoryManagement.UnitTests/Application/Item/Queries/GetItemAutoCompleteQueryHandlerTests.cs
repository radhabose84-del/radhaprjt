using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetItemAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetItemAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupDefaults(List<GetItemAutoCompleteDto> items)
        {
            _mockRepo.Setup(r => r.GetItemAutoCompleteAsync(
                    It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);
            _mockMapper.Setup(m => m.Map<List<GetItemAutoCompleteDto>>(It.IsAny<object>())).Returns(items);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var items = new List<GetItemAutoCompleteDto> { new GetItemAutoCompleteDto { Id = 1 } };
            SetupDefaults(items);

            var result = await CreateSut().Handle(new GetItemAutoCompleteQuery { SearchPattern = "item" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmpty()
        {
            SetupDefaults(new List<GetItemAutoCompleteDto>());

            var result = await CreateSut().Handle(new GetItemAutoCompleteQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            SetupDefaults(new List<GetItemAutoCompleteDto>());

            await CreateSut().Handle(new GetItemAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetItemAutoCompleteAsync(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
