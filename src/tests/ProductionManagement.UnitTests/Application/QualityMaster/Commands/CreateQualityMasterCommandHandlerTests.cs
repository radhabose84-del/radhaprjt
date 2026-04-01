using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster;

namespace ProductionManagement.UnitTests.Application.QualityMaster.Commands
{
    public sealed class CreateQualityMasterCommandHandlerTests
    {
        private readonly Mock<IQualityMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateQualityMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.QualityMaster>(It.IsAny<CreateQualityMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.QualityMaster());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.QualityMaster>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateQualityMasterCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(77);
            var result = await CreateSut().Handle(new CreateQualityMasterCommand(), CancellationToken.None);
            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateQualityMasterCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.QualityMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateQualityMasterCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
