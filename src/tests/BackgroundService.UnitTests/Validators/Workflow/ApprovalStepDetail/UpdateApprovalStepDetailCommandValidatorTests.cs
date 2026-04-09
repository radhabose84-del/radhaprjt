using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Presentation.Validation.Workflow.ApprovalStepDetail;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalStepDetail
{
    public sealed class UpdateApprovalStepDetailCommandValidatorTests
    {
        private readonly Mock<IApprovalStepDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateApprovalStepDetailCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

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

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroWorkFlowTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.WorkFlowTypeId = 0;
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WorkFlowTypeId);
        }

        [Fact]
        public async Task Validate_ZeroStepOrder_FailsValidation()
        {
            var command = ValidCommand();
            command.StepOrder = 0;
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StepOrder);
        }

        [Fact]
        public async Task Validate_ZeroApprovalStepId_FailsValidation()
        {
            var command = ValidCommand();
            command.ApprovalStepId = 0;
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ApprovalStepId);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
