using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalStepDetail.Commands
{
    public sealed class UpdateApprovalStepDetailCommandHandlerTests
    {
        private readonly Mock<IApprovalStepDetailCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateApprovalStepDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateApprovalStepDetailCommand ValidCommand() =>
            new()
            {
                Id = 1,
                WorkFlowTypeId = 1,
                StepOrder = 1,
                TargetTypeId = 1,
                TargetValueId = 10,
                ApprovalStepId = 5,
                StopOnFirstMatch = 0,
                IsActive = 1,
                IsEdit = 0,
                ApprovalStepUnitMappings = new List<ApprovalStepUnitMappingUpdateDto>()
            };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>(It.IsAny<UpdateApprovalStepDetailCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>()))
                .ReturnsAsync(updateResult);
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
                r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>()),
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
