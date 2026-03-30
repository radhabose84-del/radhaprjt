using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Queries
{
    public sealed class GetItemCategoryAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetItemCategoryAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var repoList = new List<ItemCategoryAutoCompleteDto>
            {
                new() { Id = 1, ItemCategoryName = "Electronics" }
            };
            var mappedList = new List<ItemCategoryAutoCompleteDto>
            {
                new() { Id = 1, ItemCategoryName = "Electronics" }
            };
            _mockQueryRepo.Setup(r => r.GetItemCategoryAutoCompleteAsync(null, "Elec", false, 0))
                .ReturnsAsync(repoList);
            _mockMapper.Setup(m => m.Map<List<ItemCategoryAutoCompleteDto>>(repoList)).Returns(mappedList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemCategoryAutoCompleteQuery { SearchPattern = "Elec" }, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ItemCategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var emptyList = new List<ItemCategoryAutoCompleteDto>();
            _mockQueryRepo.Setup(r => r.GetItemCategoryAutoCompleteAsync(null, string.Empty, false, 0))
                .ReturnsAsync(emptyList);
            _mockMapper.Setup(m => m.Map<List<ItemCategoryAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ItemCategoryAutoCompleteDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemCategoryAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetItemCategoryAutoCompleteAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ItemCategoryAutoCompleteDto>());
            _mockMapper.Setup(m => m.Map<List<ItemCategoryAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ItemCategoryAutoCompleteDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetItemCategoryAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetItemCategoryAutoCompleteAsync(
                It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
        }
    }
}
