using FluentValidation.TestHelper;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Presentation.Validation.ExchangeRate;

namespace PurchaseManagement.UnitTests.Validators.ExchangeRate
{
    public sealed class ExchangeRateCommandValidatorTests
    {
        private ExchangeRateCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new ExchangeRateCommand("USD", new[] { "EUR", "INR" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyBaseCurrency_FailsValidation(string? baseCurrency)
        {
            var command = new ExchangeRateCommand(baseCurrency!, new[] { "EUR" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BaseCurrency);
        }

        [Theory]
        [InlineData("US")]
        [InlineData("USDD")]
        [InlineData("usd")]
        public async Task Validate_InvalidBaseCurrencyFormat_FailsValidation(string baseCurrency)
        {
            var command = new ExchangeRateCommand(baseCurrency, new[] { "EUR" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BaseCurrency);
        }

        [Fact]
        public async Task Validate_EmptySymbols_FailsValidation()
        {
            var command = new ExchangeRateCommand("USD", Array.Empty<string>());

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_QuoteCurrencySameAsBase_FailsValidation()
        {
            var command = new ExchangeRateCommand("USD", new[] { "USD" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("eu")]
        [InlineData("EURO")]
        public async Task Validate_InvalidQuoteCurrencyFormat_FailsValidation(string quoteCurrency)
        {
            var command = new ExchangeRateCommand("USD", new[] { quoteCurrency });

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
