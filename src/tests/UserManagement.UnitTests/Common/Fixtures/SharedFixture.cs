namespace UserManagement.UnitTests.Common.Fixtures
{
    public class SharedFixture : IClassFixture<TestFixture>
    {
        protected readonly TestFixture Fx;
        protected SharedFixture(TestFixture fx) => Fx = fx;
    }
}