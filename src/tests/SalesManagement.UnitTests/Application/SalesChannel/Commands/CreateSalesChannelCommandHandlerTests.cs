#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.SalesChannel.Commands
{
    public class CreateSalesChannelCommandHandlerTests
    {
        private readonly Mock<ISalesChannelCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateSalesChannelCommandHandler CreateSut() =>
            new CreateSalesChannelCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Helper ────────────────────────────────────────────────────────────

        private void SetupHappyPath(CreateSalesChannelCommand command, int newId = 1)
        {
            var entity = SalesChannelBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            const int expectedId = 42;
            SetupHappyPath(command, newId: expectedId);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesChannel>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = SalesChannelBuilders.ValidCreateCommand(code: "CH001");
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "SALES_CHANNEL_CREATE" &&
                        e.Module == "SalesChannel"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsChannelCode()
        {
            var command = SalesChannelBuilders.ValidCreateCommand(code: "CH001");
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "CH001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.SalesChannel capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.SalesChannel
            {
                SalesChannelCode = command.SalesChannelCode,
                SalesChannelName = command.SalesChannelName
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesChannel>()))
                .Callback<SalesManagement.Domain.Entities.SalesChannel>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsIsDeletedNotDeleted()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.SalesChannel capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.SalesChannel
            {
                SalesChannelCode = command.SalesChannelCode,
                SalesChannelName = command.SalesChannelName
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesChannel>()))
                .Callback<SalesManagement.Domain.Entities.SalesChannel>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
