namespace PurchaseManagement.UnitTests.Domain
{
    public class ExchangeRateEntityTests
    {
        [Fact]
        public void ExchangeRate_DefaultIsActive_ShouldBeTrue()
        {
            var entity = new PurchaseManagement.Domain.Entities.ExchangeRate();
            entity.IsActive.Should().BeTrue();
        }

        [Fact]
        public void ExchangeRate_DefaultSource_ShouldBeFrankfurter()
        {
            var entity = new PurchaseManagement.Domain.Entities.ExchangeRate();
            entity.Source.Should().Be("Frankfurter");
        }

        [Fact]
        public void ExchangeRate_Properties_ShouldBeAssignable()
        {
            var effectiveDate = new DateOnly(2025, 6, 1);
            var entity = new PurchaseManagement.Domain.Entities.ExchangeRate
            {
                Id = 1,
                BaseCurrency = "INR",
                QuoteCurrency = "USD",
                Rate = 0.012m,
                ActualRate = 83.33m,
                EffectiveDate = effectiveDate,
                Source = "Frankfurter",
                IsActive = true
            };

            entity.Id.Should().Be(1);
            entity.BaseCurrency.Should().Be("INR");
            entity.QuoteCurrency.Should().Be("USD");
            entity.Rate.Should().Be(0.012m);
            entity.ActualRate.Should().Be(83.33m);
            entity.EffectiveDate.Should().Be(effectiveDate);
        }

        [Fact]
        public void ExchangeRate_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseManagement.Domain.Entities.ExchangeRate
            {
                ActualRate = null,
                ModifiedOnUtc = null
            };

            entity.ActualRate.Should().BeNull();
            entity.ModifiedOnUtc.Should().BeNull();
        }
    }
}
