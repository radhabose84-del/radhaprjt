using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Queries.GetProcessMasterById;
using ProductionManagement.Application.ProcessMaster.Dto;

namespace ProductionManagement.UnitTests.Application.ProcessMaster.Queries
{
    public sealed class GetProcessMasterByIdQueryHandlerTests
    {
        private readonly Mock<IProcessMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProcessMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new ProcessMasterDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<ProcessMasterDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetProcessMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProcessMasterDto?)null);

            var result = await CreateSut().Handle(new GetProcessMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
