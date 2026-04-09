using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.WorkflowType.Commands
{
    public sealed class UpdateWorkflowTypeCommandHandlerTests
    {
        private readonly Mock<IWorkflowTypeCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateWorkflowTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateWorkflowTypeCommand ValidCommand() =>
            new()
            {
                Id = 1,
                ModuleId = 1,
                MenuId = 100,
                HasLine = 1,
                IsMultiselect = 0,
                IsActive = 1
            };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Workflow.WorkflowType>(It.IsAny<UpdateWorkflowTypeCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Workflow.WorkflowType { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.WorkflowType>()))
                .ReturnsAsync(updateResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.WorkflowType>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "WORKFLOW_TYPE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            SetupHappyPath(false);
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
