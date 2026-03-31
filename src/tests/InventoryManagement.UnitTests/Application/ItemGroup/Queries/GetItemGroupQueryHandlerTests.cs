using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemGroup.Queries
{
    public sealed class GetItemGroupQueryHandlerTests
    {
        private readonly Mock<IItemGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyDynamic = Enumerable.Empty<dynamic>();
            _mockQueryRepo
                .Setup(r => r.GetAllItemGroupAsync(1, 15, null))
                .ReturnsAsync((emptyDynamic, 0));
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupDto>>(It.IsAny<object>()))
                .Returns(new List<ItemGroupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemGroupQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectData()
        {
            var emptyDynamic = Enumerable.Empty<dynamic>();
            var dtos = new List<ItemGroupDto>
            {
                ItemGroupBuilders.ValidDto(1),
                ItemGroupBuilders.ValidDto(2)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllItemGroupAsync(1, 10, null))
                .ReturnsAsync((emptyDynamic, 2));
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemGroupQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var emptyDynamic = Enumerable.Empty<dynamic>();
            _mockQueryRepo
                .Setup(r => r.GetAllItemGroupAsync(2, 5, "search"))
                .ReturnsAsync((emptyDynamic, 11));
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupDto>>(It.IsAny<object>()))
                .Returns(new List<ItemGroupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemGroupQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var emptyDynamic = Enumerable.Empty<dynamic>();
            _mockQueryRepo
                .Setup(r => r.GetAllItemGroupAsync(1, 15, null))
                .ReturnsAsync((emptyDynamic, 0));
            _mockMapper
                .Setup(m => m.Map<List<ItemGroupDto>>(It.IsAny<object>()))
                .Returns(new List<ItemGroupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetItemGroupQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllItemGroupAsync(1, 15, null),
                Times.Once);
        }
    }
}
