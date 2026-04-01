using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.UpdateLotMaster;

namespace ProductionManagement.UnitTests.Application.LotMaster.Commands
{
    public sealed class UpdateLotMasterCommandHandlerTests
    {
        private readonly Mock<ILotMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateLotMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.LotMaster>(It.IsAny<UpdateLotMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.LotMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()))
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateLotMasterCommand { Id = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateLotMasterCommand { Id = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateLotMasterCommand { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
