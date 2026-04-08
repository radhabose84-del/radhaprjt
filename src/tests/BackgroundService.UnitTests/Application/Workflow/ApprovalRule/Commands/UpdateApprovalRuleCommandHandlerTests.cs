using AutoMapper;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRule.Commands
{
    public sealed class UpdateApprovalRuleCommandHandlerTests
    {
        private readonly Mock<IApprovalRuleCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateApprovalRuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateApprovalRuleCommand ValidCommand() =>
            new()
            {
                Id = 1,
                ActionId = 1,
                ApprovalStepDetailId = 10,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Priority = 1,
                IsActive = 1,
                ApprovalRuleConditions = new List<ApprovalRuleConditionUpdateDto>()
            };

        private void SetupHappyPath()
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Workflow.ApprovalRule>(It.IsAny<UpdateApprovalRuleCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Workflow.ApprovalRule { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.ApprovalRule>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.ApprovalRule>()),
                Times.Once);
        }
    }
}
