using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using UserManagement.Presentation.Validation.DepartmentGroup;

namespace UserManagement.UnitTests.Validators.DepartmentGroup
{
    public sealed class DeleteDepartmentGroupCommandValidatorTests
    {
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteDepartmentGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = new DeleteDepartmentGroupCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var command = new DeleteDepartmentGroupCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_DepartmentGroupHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = new DeleteDepartmentGroupCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_DepartmentGroupNoDependencies_PassesSoftDeleteValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(5))
                .ReturnsAsync(false);

            var command = new DeleteDepartmentGroupCommand { Id = 5 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
