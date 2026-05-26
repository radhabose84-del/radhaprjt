using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.MiscMaster.Commands
{
    public class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new UpdateMiscMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        private void SetupHappyPath(UpdateMiscMasterCommand command, int updatedId = 1)
        {
            var entity = MiscMasterBuilders.ValidEntity(command.Id);

            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.MiscMaster>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(updatedId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(id: 5);
            SetupHappyPath(command, updatedId: 5);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<QCManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "MISC_MASTER_UPDATE" &&
                        e.Module == "MiscMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWithLinkedDependents_ThrowsExceptionRules()
        {
            // Future QualityParameter will be checked. Today the repo returns false,
            // so we explicitly arrange it to return true to verify the guard fires.
            var command = MiscMasterBuilders.ValidUpdateCommand(id: 1, isActive: 0);
            _mockQueryRepo.Setup(r => r.IsMiscMasterLinkedAsync(1)).ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*");
        }
    }
}
