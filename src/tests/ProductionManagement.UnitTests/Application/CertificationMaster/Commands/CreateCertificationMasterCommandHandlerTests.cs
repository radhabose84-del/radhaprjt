using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;

namespace ProductionManagement.UnitTests.Application.CertificationMaster.Commands
{
    public sealed class CreateCertificationMasterCommandHandlerTests
    {
        private readonly Mock<ICertificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateCertificationMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.CertificationMaster>(
                    It.IsAny<CreateCertificationMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.CertificationMaster());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.CertificationMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            SetupHappyPath(1);
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            var result = await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            // Arrange
            SetupHappyPath(42);
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            var result = await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            var result = await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            result.Message.Should().Be("Certification Master created successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.CertificationMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesCorrectAuditActionDetail()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "CERTIFICATIONMASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsMapperOnce()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateCertificationMasterCommand { CertificationName = "ISO 9001" };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<ProductionManagement.Domain.Entities.CertificationMaster>(
                    It.IsAny<CreateCertificationMasterCommand>()),
                Times.Once);
        }
    }
}
