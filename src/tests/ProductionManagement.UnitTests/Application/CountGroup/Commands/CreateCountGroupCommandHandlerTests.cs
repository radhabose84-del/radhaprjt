using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Commands.CreateCountGroup;

namespace ProductionManagement.UnitTests.Application.CountGroup.Commands
{
    public sealed class CreateCountGroupCommandHandlerTests
    {
        private readonly Mock<ICountGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateCountGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.CountGroup>(It.IsAny<CreateCountGroupCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.CountGroup());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.CountGroup>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateCountGroupCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(77);
            var result = await CreateSut().Handle(new CreateCountGroupCommand(), CancellationToken.None);
            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateCountGroupCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.CountGroup>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateCountGroupCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
