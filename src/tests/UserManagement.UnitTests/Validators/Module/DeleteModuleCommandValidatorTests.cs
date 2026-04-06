using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Commands.DeleteModule;
using UserManagement.Presentation.Validation.Module;

namespace UserManagement.UnitTests.Validators.Module
{
    public sealed class DeleteModuleCommandValidatorTests
    {
        private readonly Mock<IModuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteModuleCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = new DeleteModuleCommand { ModuleId = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroModuleId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var command = new DeleteModuleCommand { ModuleId = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_ModuleHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = new DeleteModuleCommand { ModuleId = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_ModuleNoDependencies_PassesSoftDeleteValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(5))
                .ReturnsAsync(false);

            var command = new DeleteModuleCommand { ModuleId = 5 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ModuleId);
        }
    }
}
