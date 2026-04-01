using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.UpdateRepacking;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Application.Repacking.Commands
{
    public sealed class UpdateRepackingCommandHandlerTests
    {
        private readonly Mock<IRepackingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRepackingCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<RepackingHeader>(It.IsAny<UpdateRepackingCommand>()))
                .Returns(new RepackingHeader());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<RepackingHeader>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateRepackingCommand { Id = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateRepackingCommand { Id = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<RepackingHeader>()), Times.Once);
        }
    }
}
