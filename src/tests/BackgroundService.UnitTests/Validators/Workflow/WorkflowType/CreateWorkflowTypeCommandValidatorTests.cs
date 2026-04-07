using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.Workflow.WorkflowTypes;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.WorkflowType
{
    public sealed class CreateWorkflowTypeCommandValidatorTests
    {
        private readonly Mock<IWorkflowTypeQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateWorkflowTypeCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private static CreateWorkflowTypeCommand ValidCommand() =>
            new()
            {
                ModuleId = 1,
                MenuId = 100,
                HasLine = 1,
                IsMultiselect = 0
            };

        private void SetupAllAsyncMocks(int menuId = 100, int moduleId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(menuId, moduleId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.MenuId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroMenuId_FailsValidation()
        {
            var command = ValidCommand();
            command.MenuId = 0;
            // AlreadyExists async rule still runs
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public async Task Validate_AlreadyExists_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MenuId, command.ModuleId, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
