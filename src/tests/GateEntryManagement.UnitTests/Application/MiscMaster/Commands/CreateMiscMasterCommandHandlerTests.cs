using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using GateEntryManagement.Domain.Events;
using GateEntryManagement.UnitTests.TestData;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateMiscMasterCommand command, int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<GateEntryManagement.Domain.Entities.MiscMaster>(It.IsAny<CreateMiscMasterCommand>()))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.GetMaxSortOrderAsync(command.MiscTypeId))
                .ReturnsAsync(0);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateEntryManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Misc Master created successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<GateEntryManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "MISC_MASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CalculatesSortOrder()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            GateEntryManagement.Domain.Entities.MiscMaster? capturedEntity = null;

            _mockMapper
                .Setup(m => m.Map<GateEntryManagement.Domain.Entities.MiscMaster>(It.IsAny<CreateMiscMasterCommand>()))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.GetMaxSortOrderAsync(command.MiscTypeId))
                .ReturnsAsync(5);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateEntryManagement.Domain.Entities.MiscMaster>()))
                .Callback<GateEntryManagement.Domain.Entities.MiscMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.SortOrder.Should().Be(6);
        }
    }
}
