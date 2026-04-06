using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Commands.DeleteCompany;
using UserManagement.Presentation.Validation.Companies;

namespace UserManagement.UnitTests.Validators.Companies
{
    public sealed class DeleteCompanyCommandValidatorTests
    {
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteCompanyCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = new DeleteCompanyCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var command = new DeleteCompanyCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_CompanyHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = new DeleteCompanyCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_CompanyNoDependencies_PassesSoftDeleteValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(5))
                .ReturnsAsync(false);

            var command = new DeleteCompanyCommand { Id = 5 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
