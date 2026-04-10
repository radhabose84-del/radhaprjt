using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterById;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationMaster.Queries
{
    public sealed class GetItemSpecificationMasterByIdQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemSpecificationMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = ItemSpecificationMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<ItemSpecificationMasterDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemSpecificationMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ItemSpecificationMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetItemSpecificationMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
