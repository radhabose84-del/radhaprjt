using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterById;
using ProductionManagement.Application.YarnTwistMaster.Dto;

namespace ProductionManagement.UnitTests.Application.YarnTwistMaster.Queries
{
    public sealed class GetYarnTwistMasterByIdQueryHandlerTests
    {
        private readonly Mock<IYarnTwistMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetYarnTwistMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new YarnTwistMasterDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<YarnTwistMasterDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetYarnTwistMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((YarnTwistMasterDto?)null);

            var result = await CreateSut().Handle(new GetYarnTwistMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
