using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.Item;

namespace MaintenanceManagement.IntegrationTests.Repositories.Item
{
    /// <summary>
    /// ItemQueryRepository calls stored procedures [dbo].[GetGroupCode] and [dbo].[GetItemsByGroupCode]
    /// that do NOT exist in the freshly-created test database. These tests verify:
    ///   1. The repository can be constructed with its real dependencies
    ///   2. Its SP-backed methods throw a SqlException (as expected when the SP is missing)
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ItemQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ItemQueryRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetGroupCodes_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetGroupCodes("U001");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetItemMasters_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetItemMasters("U001", "GRP001", null, null);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetItemMasters_With_Filter_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetItemMasters("U001", "GRP001", "ITEM001", "Test Item");

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
