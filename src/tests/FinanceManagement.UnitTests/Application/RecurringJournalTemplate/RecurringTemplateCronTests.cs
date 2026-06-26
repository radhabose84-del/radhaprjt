using FinanceManagement.Application.Common;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate
{
    public sealed class RecurringTemplateCronTests
    {
        [Fact]
        public void Monthly_AnchorsOnStartDay() =>
            RecurringTemplateCron.For("MONTHLY", new DateOnly(2026, 7, 15)).Should().Be("0 0 15 * *");

        [Fact]
        public void Monthly_ClampsDayTo28() =>
            RecurringTemplateCron.For("MONTHLY", new DateOnly(2026, 1, 31)).Should().Be("0 0 28 * *");

        [Fact]
        public void Annually_UsesStartMonthAndDay() =>
            RecurringTemplateCron.For("ANNUALLY", new DateOnly(2026, 4, 1)).Should().Be("0 0 1 4 *");

        [Fact]
        public void Quarterly_StepsThreeMonthsFromStart() =>
            RecurringTemplateCron.For("QUARTERLY", new DateOnly(2026, 1, 15)).Should().Be("0 0 15 1,4,7,10 *");

        [Fact]
        public void Daily_FiresEveryDay() =>
            RecurringTemplateCron.For("DAILY", new DateOnly(2026, 7, 15)).Should().Be("0 0 * * *");

        [Fact]
        public void Unknown_DefaultsToMonthly() =>
            RecurringTemplateCron.For("WHATEVER", new DateOnly(2026, 7, 10)).Should().Be("0 0 10 * *");
    }
}
