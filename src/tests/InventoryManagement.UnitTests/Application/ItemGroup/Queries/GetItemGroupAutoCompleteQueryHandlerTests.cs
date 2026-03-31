using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemGroup.Queries
{
    public sealed class GetItemGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IItemGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var lookupList = new List<ItemGroupAutoCompleteDto>
            {
                ItemGroupBuilders.ValidAutoCompleteDto(1, "Electronics")
            };

            _mockQueryRepo
                .Setup(r => r.GetItemGroupAutoCompleteAsync("Elec"))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(lookupList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemGroupAutoCompleteQuery { SearchPattern = "Elec" }, CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetItemGroupAutoCompleteAsync("xyz"))
                .ReturnsAsync(new List<ItemGroupAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ItemGroupAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemGroupAutoCompleteQuery { SearchPattern = "xyz" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetItemGroupAutoCompleteAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ItemGroupAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ItemGroupAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetItemGroupAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetItemGroupAutoCompleteAsync(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullSearchPattern_PassesEmptyStringToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.GetItemGroupAutoCompleteAsync(string.Empty))
                .ReturnsAsync(new List<ItemGroupAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ItemGroupAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemGroupAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
