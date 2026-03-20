using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Presentation.Validation.Divisions;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.Division
{
    public sealed class DeleteDivisionCommandValidatorTests
    {
        private readonly Mock<IDivisionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteDivisionCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = DivisionBuilders.ValidDeleteCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteDivisionCommand { Id = 0 };

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_DivisionHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = DivisionBuilders.ValidDeleteCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_DivisionNoDependencies_PassesSoftDeleteValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(5))
                .ReturnsAsync(false);

            var command = new DeleteDivisionCommand { Id = 5 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
