using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using BackgroundService.Presentation.Validation.Workflow.WorkflowTypes;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.WorkflowType
{
    public sealed class UpdateWorkflowTypeCommandValidatorTests
    {
        private readonly Mock<IWorkflowTypeQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateWorkflowTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

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

        private void SetupAllAsyncMocks(int menuId = 100, int moduleId = 1, int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(menuId, moduleId, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.MenuId, command.ModuleId, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroModuleId_FailsValidation()
        {
            var command = ValidCommand();
            command.ModuleId = 0;
            SetupAllAsyncMocks(command.MenuId, command.ModuleId, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_ZeroMenuId_FailsValidation()
        {
            var command = ValidCommand();
            command.MenuId = 0;
            SetupAllAsyncMocks(command.MenuId, command.ModuleId, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MenuId, command.ModuleId, command.Id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_AlreadyExists_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MenuId, command.ModuleId, command.Id))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
