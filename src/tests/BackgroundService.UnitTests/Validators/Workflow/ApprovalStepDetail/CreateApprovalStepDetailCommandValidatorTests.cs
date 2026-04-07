using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Presentation.Validation.Workflow.ApprovalStepDetail;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalStepDetail
{
    public sealed class CreateApprovalStepDetailCommandValidatorTests
    {
        private readonly Mock<IApprovalStepDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateApprovalStepDetailCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private static CreateApprovalStepDetailCommand ValidCommand() =>
            new()
            {
                WorkFlowTypeId = 1,
                StepOrder = 1,
                TargetTypeId = 1,
                TargetValueId = 10,
                ApprovalStepId = 5,
                StopOnFirstMatch = 0,
                IsEdit = 0,
                ApprovalStepUnitMappings = new List<ApprovalStepUnitMappingDto>()
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroWorkFlowTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.WorkFlowTypeId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WorkFlowTypeId);
        }

        [Fact]
        public async Task Validate_ZeroStepOrder_FailsValidation()
        {
            var command = ValidCommand();
            command.StepOrder = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StepOrder);
        }

        [Fact]
        public async Task Validate_ZeroApprovalStepId_FailsValidation()
        {
            var command = ValidCommand();
            command.ApprovalStepId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ApprovalStepId);
        }
    }
}
