using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionById;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.UnitTests.Application.ProductionPack.Queries
{
    public sealed class GetProductionByIdQueryHandlerTests
    {
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProductionByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new ProductionPackHeaderDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<ProductionPackHeaderDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetProductionByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProductionPackHeaderDto?)null);

            var result = await CreateSut().Handle(new GetProductionByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
