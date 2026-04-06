using FluentValidation.TestHelper;
using PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;
using PurchaseManagement.Presentation.Validation.ExchangeRate;

namespace PurchaseManagement.UnitTests.Validators.ExchangeRate
{
    public sealed class GetLatestRateQueryValidatorTests
    {
        private GetLatestRateQueryValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidQuery_PassesValidation()
        {
            var query = new GetLatestRateQuery("USD", "EUR");

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyBaseCurrency_FailsValidation(string? baseCurrency)
        {
            var query = new GetLatestRateQuery(baseCurrency!, "EUR");

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x.BaseCurrency);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyQuoteCurrency_FailsValidation(string? quoteCurrency)
        {
            var query = new GetLatestRateQuery("USD", quoteCurrency!);

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x.QuoteCurrency);
        }

        [Fact]
        public async Task Validate_SameCurrencies_FailsValidation()
        {
            var query = new GetLatestRateQuery("USD", "USD");

            var result = await CreateValidator().TestValidateAsync(query);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("us")]
        [InlineData("USDD")]
        [InlineData("usd")]
        public async Task Validate_InvalidBaseCurrencyFormat_FailsValidation(string baseCurrency)
        {
            var query = new GetLatestRateQuery(baseCurrency, "EUR");

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x.BaseCurrency);
        }
    }
}
