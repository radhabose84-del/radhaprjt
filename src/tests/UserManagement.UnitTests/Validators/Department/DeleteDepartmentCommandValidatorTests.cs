using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Presentation.Validation.Department;

namespace UserManagement.UnitTests.Validators.Department
{
    public sealed class DeleteDepartmentCommandValidatorTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteDepartmentCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteDepartmentCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteDepartmentCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task LinkedRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteDepartmentCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
