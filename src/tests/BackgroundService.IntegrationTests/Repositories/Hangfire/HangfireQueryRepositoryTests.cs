using Microsoft.Data.SqlClient;
using BackgroundService.Infrastructure.Repositories.HangFire;

namespace BackgroundService.IntegrationTests.Repositories.Hangfire
{
    [Collection("DatabaseCollection")]
    public sealed class HangfireQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public HangfireQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new HangfireQueryRepository(conn);
            repo.Should().NotBeNull();
        }
    }
}
