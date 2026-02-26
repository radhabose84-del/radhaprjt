using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.SalesGroup.Commands
{
    public class CreateSalesGroupCommandHandlerTests
    {
        private readonly Mock<ISalesGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateSalesGroupCommandHandler CreateSut() =>
            new CreateSalesGroupCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetupHappyPath(CreateSalesGroupCommand command, int newId = 1)
        {
            var entity = SalesGroupBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command))
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
            var command = SalesGroupBuilders.ValidCreateCommand();
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
            var command = SalesGroupBuilders.ValidCreateCommand();
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
            var command = SalesGroupBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidCreateCommand(name: "Test Sales Group");
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "SALES_GROUP_CREATE" &&
                        e.Module == "SalesGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsGroupName()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidCreateCommand(name: "North Region Group");
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "North Region Group"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidCreateCommand();
            var capturedEntity = default(SalesManagement.Domain.Entities.SalesGroup);

            var entity = new SalesManagement.Domain.Entities.SalesGroup
            {
                SalesGroupName = command.SalesGroupName,
                SalesOfficeId = command.SalesOfficeId,
                ResponsibleManager = command.ResponsibleManager,
                RegionTerritory = command.RegionTerritory
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesGroup>()))
                .Callback<SalesManagement.Domain.Entities.SalesGroup>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsIsDeletedNotDeleted()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidCreateCommand();
            var capturedEntity = default(SalesManagement.Domain.Entities.SalesGroup);

            var entity = new SalesManagement.Domain.Entities.SalesGroup
            {
                SalesGroupName = command.SalesGroupName,
                SalesOfficeId = command.SalesOfficeId,
                ResponsibleManager = command.ResponsibleManager,
                RegionTerritory = command.RegionTerritory
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesGroup>()))
                .Callback<SalesManagement.Domain.Entities.SalesGroup>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
