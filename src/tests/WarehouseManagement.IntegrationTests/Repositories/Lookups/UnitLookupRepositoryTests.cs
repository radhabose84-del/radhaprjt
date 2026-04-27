using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Lookups;

namespace WarehouseManagement.IntegrationTests.Repositories.Lookups
{
    /// <summary>
    /// UnitLookupRepository queries [AppData].[Unit], [AppSecurity].[UserUnit], and
    /// [AppSecurity].[Users] which live in the UserManagement module's schemas. The
    /// Warehouse test database only provisions the Warehouse schema (per CLAUDE.md
    /// cross-module isolation rule), so these cross-schema tables do not exist in
    /// this test DB.
    ///
    /// These tests verify:
    ///   1. The repository can be constructed with its real dependencies.
    ///   2. Empty/null input is handled defensively before any SQL execution.
    ///   3. SQL methods throw SqlException when the cross-schema tables are missing.
    /// Full data-flow coverage lives in UserManagement integration tests.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class UnitLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UnitLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UnitLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UnitLookupRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_SqlException_When_AppData_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByIdAsync(1);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var repo = CreateRepo();

            var result = await repo.GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var repo = CreateRepo();

            var result = await repo.GetByIdsAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Filter_Out_Invalid_Ids_Before_Empty_Result()
        {
            var repo = CreateRepo();

            // Negative and zero ids are filtered out via .Where(id => id > 0)
            // → empty result, no SQL hit, no exception.
            var result = await repo.GetByIdsAsync(new[] { 0, -1, -999 });

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Throw_SqlException_When_AppData_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByIdsAsync(new[] { 1, 2, 3 });

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetAllUnitAsync_Should_Throw_SqlException_When_AppData_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetAllUnitAsync();

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetUserUnitAsync_Should_Throw_SqlException_When_AppSecurity_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetUserUnitAsync(1);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetUserUnitByUserNameAsync_Should_Throw_SqlException_When_AppSecurity_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetUserUnitByUserNameAsync("test-user");

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
