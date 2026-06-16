using Shared.QAInfrastructure.Fixtures;
using Xunit;

namespace FinanceManagement.FunctionalTests
{
    [CollectionDefinition("US-GL02-03A")]
    public sealed class ScheduleIIIStoryCollection : ICollectionFixture<QAServerFixture> { }
}
