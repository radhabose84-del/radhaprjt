using Shared.QAInfrastructure.Fixtures;
using Xunit;

namespace FinanceManagement.FunctionalTests
{
    [CollectionDefinition("US-GL02-03A")]
    public sealed class ScheduleIIIStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL02-05")]
    public sealed class TaxCodeStoryCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("US-GL02-02")]
    public sealed class AccountGroupStoryCollection : ICollectionFixture<QAServerFixture> { }
}
