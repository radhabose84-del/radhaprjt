using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueById;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationValue.Queries
{
    public sealed class GetItemSpecificationValueByIdQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemSpecificationValueByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = ItemSpecificationValueBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<ItemSpecificationValueDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ItemSpecificationValueDto?)null);

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
