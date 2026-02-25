#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Commands
{
    public class CreateSalesOrganisationCommandHandlerTests
    {
        private readonly Mock<ISalesOrganisationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateSalesOrganisationCommandHandler CreateSut() =>
            new CreateSalesOrganisationCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetupHappyPath(CreateSalesOrganisationCommand command, int newId = 1)
        {
            var entity = SalesOrganisationBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(
                    It.IsAny<AuditLogsDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            const int expectedId = 42;
            SetupHappyPath(command, newId: expectedId);
            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrganisation>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: "ORG001");
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "SALES_ORG_CREATE" &&
                        e.Module == "SalesOrganisation"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsOrganisationCode()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: "ORG001");
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "ORG001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            var capturedEntity = default(SalesManagement.Domain.Entities.SalesOrganisation);

            var entity = new SalesManagement.Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = command.SalesOrganisationCode,
                SalesOrganisationName = command.SalesOrganisationName,
                CompanyId = command.CompanyId
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrganisation>()))
                .Callback<SalesManagement.Domain.Entities.SalesOrganisation>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsIsDeletedNotDeleted()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            var capturedEntity = default(SalesManagement.Domain.Entities.SalesOrganisation);

            var entity = new SalesManagement.Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = command.SalesOrganisationCode,
                SalesOrganisationName = command.SalesOrganisationName,
                CompanyId = command.CompanyId
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrganisation>()))
                .Callback<SalesManagement.Domain.Entities.SalesOrganisation>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
