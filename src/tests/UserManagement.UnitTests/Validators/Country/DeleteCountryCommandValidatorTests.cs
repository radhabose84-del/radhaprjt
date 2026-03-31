using FluentValidation.TestHelper;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Presentation.Validation.Country;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.Country
{
    public sealed class DeleteCountryCommandValidatorTests
    {
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteCountryCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = CountryBuilders.ValidDeleteCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var command = new DeleteCountryCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_CountryHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = CountryBuilders.ValidDeleteCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
