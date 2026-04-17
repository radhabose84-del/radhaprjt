using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType;

namespace ProductionManagement.UnitTests.Application.RawMaterialType.Commands
{
    public sealed class CreateRawMaterialTypeCommandHandlerTests
    {
        private readonly Mock<IRawMaterialTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateRawMaterialTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RawMaterialType>(It.IsAny<CreateRawMaterialTypeCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RawMaterialType());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RawMaterialType>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateRawMaterialTypeCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(99);
            var result = await CreateSut().Handle(new CreateRawMaterialTypeCommand(), CancellationToken.None);
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateRawMaterialTypeCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RawMaterialType>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateRawMaterialTypeCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "RAWMATERIALTYPE_CREATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
