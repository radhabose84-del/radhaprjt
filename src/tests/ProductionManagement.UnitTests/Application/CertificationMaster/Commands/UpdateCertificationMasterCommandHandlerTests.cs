using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster;

namespace ProductionManagement.UnitTests.Application.CertificationMaster.Commands
{
    public sealed class UpdateCertificationMasterCommandHandlerTests
    {
        private readonly Mock<ICertificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateCertificationMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.CertificationMaster>(
                    It.IsAny<UpdateCertificationMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.CertificationMaster { Id = updatedId });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.CertificationMaster>()))
                .ReturnsAsync(updatedId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            SetupHappyPath(1);
            var command = new UpdateCertificationMasterCommand
            {
                Id = 1,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

            // Act
            var result = await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            // Arrange
            SetupHappyPath(7);
            var command = new UpdateCertificationMasterCommand
            {
                Id = 7,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

            // Act
            var result = await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            // Arrange
            SetupHappyPath();
            var command = new UpdateCertificationMasterCommand
            {
                Id = 1,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

            // Act
            var result = await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            result.Message.Should().Be("Certification Master updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            // Arrange
            SetupHappyPath();
            var command = new UpdateCertificationMasterCommand
            {
                Id = 1,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.CertificationMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            SetupHappyPath();
            var command = new UpdateCertificationMasterCommand
            {
                Id = 1,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

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
            var command = new UpdateCertificationMasterCommand
            {
                Id = 1,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "CERTIFICATIONMASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsMapperOnce()
        {
            // Arrange
            SetupHappyPath();
            var command = new UpdateCertificationMasterCommand
            {
                Id = 1,
                CertificationName = "Updated Certification",
                IsActive = 1
            };

            // Act
            await CreateSut().Handle(command, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<ProductionManagement.Domain.Entities.CertificationMaster>(
                    It.IsAny<UpdateCertificationMasterCommand>()),
                Times.Once);
        }
    }
}
