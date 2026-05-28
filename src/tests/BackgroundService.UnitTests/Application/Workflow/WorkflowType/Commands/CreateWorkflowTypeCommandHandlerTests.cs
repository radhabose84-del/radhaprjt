using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.WorkflowType.Commands
{
    public sealed class CreateWorkflowTypeCommandHandlerTests
    {
        private readonly Mock<IWorkflowTypeCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateWorkflowTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateWorkflowTypeCommand ValidCommand() =>
            new()
            {
                ModuleId = 1,
                MenuId = 100,
                HasLine = 1,
                IsMultiselect = 0
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Workflow.WorkflowType>(It.IsAny<CreateWorkflowTypeCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Workflow.WorkflowType());

            var returnedIds = newId > 0 ? new List<int> { newId } : new List<int>();
            _mockCommandRepo
                .Setup(r => r.CreateBulkAsync(It.IsAny<List<BackgroundService.Domain.Entities.Workflow.WorkflowType>>()))
                .ReturnsAsync(returnedIds);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.Should().Contain(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateBulkAsync(It.IsAny<List<BackgroundService.Domain.Entities.Workflow.WorkflowType>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "WORKFLOW_TYPE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(0);
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
