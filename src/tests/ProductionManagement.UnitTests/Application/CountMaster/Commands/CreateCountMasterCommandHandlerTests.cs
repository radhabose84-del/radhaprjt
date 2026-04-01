using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Commands.CreateCountMaster;

namespace ProductionManagement.UnitTests.Application.CountMaster.Commands
{
    public sealed class CreateCountMasterCommandHandlerTests
    {
        private readonly Mock<ICountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateCountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.CountMaster>(It.IsAny<CreateCountMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.CountMaster());
            _mockQueryRepo.Setup(r => r.GetNextCountCodeAsync())
                .ReturnsAsync("CNT001");
            _mockQueryRepo.Setup(r => r.GetNextCountCodeAsync()).ReturnsAsync("C001");
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.CountMaster>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateCountMasterCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(77);
            var result = await CreateSut().Handle(new CreateCountMasterCommand(), CancellationToken.None);
            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateCountMasterCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.CountMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateCountMasterCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
