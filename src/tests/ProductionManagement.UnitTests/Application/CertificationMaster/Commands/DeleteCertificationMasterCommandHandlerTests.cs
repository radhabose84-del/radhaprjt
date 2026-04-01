using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster;

namespace ProductionManagement.UnitTests.Application.CertificationMaster.Commands
{
    public sealed class DeleteCertificationMasterCommandHandlerTests
    {
        private readonly Mock<ICertificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteCertificationMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1, bool softDeleteResult = true)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(softDeleteResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            // Arrange
            SetupHappyPath(1, softDeleteResult: true);

            // Act
            var result = await CreateSut().Handle(
                new DeleteCertificationMasterCommand(1), CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistingId_StillReturnsTrue()
        {
            // Arrange — delete handler ignores SoftDeleteAsync return value; always returns true
            SetupHappyPath(99, softDeleteResult: false);

            // Act
            var result = await CreateSut().Handle(
                new DeleteCertificationMasterCommand(99), CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteAsyncOnce()
        {
            // Arrange
            SetupHappyPath(1);

            // Act
            await CreateSut().Handle(
                new DeleteCertificationMasterCommand(1), CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            // Arrange
            SetupHappyPath(1);

            // Act
            await CreateSut().Handle(
                new DeleteCertificationMasterCommand(1), CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesCorrectAuditActionDetail()
        {
            // Arrange
            SetupHappyPath(1);

            // Act
            await CreateSut().Handle(
                new DeleteCertificationMasterCommand(1), CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "CERTIFICATIONMASTER_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
