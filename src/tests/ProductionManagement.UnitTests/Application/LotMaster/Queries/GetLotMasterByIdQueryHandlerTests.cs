using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Queries.GetLotMasterById;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.UnitTests.Application.LotMaster.Queries
{
    public sealed class GetLotMasterByIdQueryHandlerTests
    {
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLotMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new LotMasterDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<LotMasterDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetLotMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((LotMasterDto?)null);

            var result = await CreateSut().Handle(new GetLotMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
