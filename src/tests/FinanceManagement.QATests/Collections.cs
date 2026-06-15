using Shared.QAInfrastructure.Fixtures;
using Xunit;

namespace FinanceManagement.QATests
{
    [CollectionDefinition("ScheduleIIICollection")]
    public sealed class ScheduleIIICollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("SecurityCollection")]
    public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
}
