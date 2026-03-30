using PurchaseManagement.Application.ExchangeRate.Commands;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class ExchangeRateBuilders
    {
        public static ExchangeRateCommand ValidCommand(
            string baseCurrency = "INR",
            string[] symbols = null!) =>
            new ExchangeRateCommand(baseCurrency, symbols ?? new[] { "USD", "EUR" });

        public static ExchangeRateDto ValidDto(
            int id = 1,
            string baseCurrency = "INR",
            string quoteCurrency = "USD",
            decimal rate = 0.012m) =>
            new ExchangeRateDto
            {
                Id = id,
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency,
                Rate = rate,
                ActualRate = rate == 0m ? null : Math.Round(1m / rate, 8),
                EffectiveDate = new DateOnly(2025, 1, 1),
                Source = "Frankfurter"
            };

        public static PurchaseManagement.Domain.Entities.ExchangeRate ValidEntity(
            int id = 1,
            string baseCurrency = "INR",
            string quoteCurrency = "USD",
            decimal rate = 0.012m) =>
            new PurchaseManagement.Domain.Entities.ExchangeRate
            {
                Id = id,
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency,
                Rate = rate,
                ActualRate = Math.Round(1m / rate, 8),
                EffectiveDate = new DateOnly(2025, 1, 1),
                Source = "Frankfurter",
                IsActive = true,
                CreatedOnUtc = DateTime.UtcNow
            };
    }
}
