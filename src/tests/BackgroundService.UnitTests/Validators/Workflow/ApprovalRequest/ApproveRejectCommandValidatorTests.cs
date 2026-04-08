using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Presentation.Validation.Workflow.ApprovalRequest;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalRequest
{
    public sealed class ApproveRejectCommandValidatorTests
    {
        private ApproveRejectCommandValidator CreateValidator() => new();

        private static ApproveApprovalRequestCommand ValidCommand() =>
            new()
            {
                ApprovalRequestHeaderId = 1,
                ModuleTransactionId = 10,
                Remark = "Approved",
                IsApproved = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroHeaderId_FailsValidation()
        {
            var command = ValidCommand();
            command.ApprovalRequestHeaderId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ApprovalRequestHeaderId);
        }

        [Fact]
        public async Task Validate_ZeroModuleTransactionId_FailsValidation()
        {
            var command = ValidCommand();
            command.ModuleTransactionId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleTransactionId);
        }

        [Fact]
        public async Task Validate_WithValidLines_PassesValidation()
        {
            var command = ValidCommand();
            command.ApprovalRequestLine = new List<ApproveApprovalRequestLineDto>
            {
                new()
                {
                    ApprovalRequestLineId = 1,
                    ApprovalRequestHeaderId = 1,
                    ModuleLineTransactionId = 100,
                    IsApproved = 1,
                    Remark = "OK"
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithInvalidLineIds_FailsValidation()
        {
            var command = ValidCommand();
            command.ApprovalRequestLine = new List<ApproveApprovalRequestLineDto>
            {
                new()
                {
                    ApprovalRequestLineId = 0,
                    ApprovalRequestHeaderId = 0,
                    ModuleLineTransactionId = 0,
                    IsApproved = 1,
                    Remark = "test"
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_NullLines_PassesValidation()
        {
            var command = ValidCommand();
            command.ApprovalRequestLine = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
