using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRule.Commands
{
    public sealed class CreateApprovalRuleCommandHandlerTests
    {
        private readonly Mock<IApprovalRuleCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateApprovalRuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateApprovalRuleCommand ValidCommand() =>
            new()
            {
                ActionId = 1,
                ApprovalStepDetailId = 10,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Priority = 1,
                ApprovalRuleConditions = new List<ApprovalRuleConditionDto>()
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Workflow.ApprovalRule>(It.IsAny<CreateApprovalRuleCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Workflow.ApprovalRule());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.ApprovalRule>()))
                .ReturnsAsync(newId);

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

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.ApprovalRule>()),
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
                        e.ActionCode == "APPROVAL_RULE_CREATE"),
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
