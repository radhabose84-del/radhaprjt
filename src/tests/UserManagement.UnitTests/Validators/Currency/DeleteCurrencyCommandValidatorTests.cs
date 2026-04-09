using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using UserManagement.Presentation.Validation.Currency;

namespace UserManagement.UnitTests.Validators.Currency
{
    public sealed class DeleteCurrencyCommandValidatorTests
    {
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteCurrencyCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteCurrencyCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteCurrencyCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task LinkedRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteCurrencyCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
