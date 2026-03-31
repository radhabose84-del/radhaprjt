using FluentValidation.TestHelper;
using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Presentation.Validation.City;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.City
{
    public sealed class DeleteCityCommandValidatorTests
    {
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteCityCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = CityBuilders.ValidDeleteCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteCityCommand { Id = 0 };

            // SoftDeleteValidation may still be called for id=0
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_CityHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = CityBuilders.ValidDeleteCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
