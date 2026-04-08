using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.WorkflowType.Commands
{
    public sealed class DeleteWorkflowTypeCommandHandlerTests
    {
        private readonly Mock<IWorkflowTypeCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteWorkflowTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(bool deleteResult = true)
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Workflow.WorkflowType>(It.IsAny<DeleteWorkflowTypeCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Workflow.WorkflowType());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Workflow.WorkflowType>()))
                .ReturnsAsync(deleteResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            var result = await sut.Handle(new DeleteWorkflowTypeCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(new DeleteWorkflowTypeCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<BackgroundService.Domain.Entities.Workflow.WorkflowType>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(new DeleteWorkflowTypeCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.ActionCode == "WORKFLOW_TYPE_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsExceptionRules()
        {
            SetupHappyPath(false);
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new DeleteWorkflowTypeCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
