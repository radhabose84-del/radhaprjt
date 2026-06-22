using Shared.QAInfrastructure.Fixtures;
using Xunit;

namespace FinanceManagement.FunctionalTests
{
    [CollectionDefinition("US-GL02-03A")]
    public sealed class ScheduleIIIStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL02-05")]
    public sealed class TaxCodeStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL02-12")]
    public sealed class CurrencyForexConfigStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL02-02")]
    public sealed class AccountGroupStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL05-01")]
    public sealed class CostCentreStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL05-02")]
    public sealed class ProfitCentreStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL01-02")]
    public sealed class VoucherTypeStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL02-FR-008a")]
    public sealed class CoaFreezeStoryCollection : ICollectionFixture<QAServerFixture> { }
}
