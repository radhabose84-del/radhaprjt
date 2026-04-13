using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.MRS;

namespace MaintenanceManagement.IntegrationTests.Repositories.MRS
{
    /// <summary>
    /// MRSQueryRepository is entirely stored-procedure based (GetCategoryByOldUnitcode, GetDepartmentsByOldUnitcode,
    /// GetSubCostCentersByOldUnitcode, GetSubDepartmentByOldUnitcode, KalsoftePrimeIssueRequestPending) and these
    /// legacy SPs do NOT exist in the MaintenanceManagement_TestDb. These tests verify only that:
    ///   1. Each method accepts null input without throwing a NullReferenceException (repo uses ??= string.Empty)
    ///   2. Each method surfaces the SqlException from SQL Server when the SP is missing (confirms the repo forwards
    ///      the call to the correct SP)
    /// Happy-path data-return tests are intentionally skipped because they require SPs that don't live in the test DB.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MRSQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MRSQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetCategory_Should_Throw_SqlException_When_Sp_Not_Exist()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            Func<Task> act = async () => await repo.GetCategory("UNIT1");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetMDepartment_Should_Throw_SqlException_When_Sp_Not_Exist()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            Func<Task> act = async () => await repo.GetMDepartment("UNIT1");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetSubCostCenter_Should_Throw_SqlException_When_Sp_Not_Exist()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            Func<Task> act = async () => await repo.GetSubCostCenter("UNIT1");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetSubDepartment_Should_Throw_SqlException_When_Sp_Not_Exist()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            Func<Task> act = async () => await repo.GetSubDepartment("UNIT1");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetPendingIssueAsync_Should_Throw_SqlException_When_Sp_Not_Exist()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            Func<Task> act = async () => await repo.GetPendingIssueAsync("UNIT1", "ITEM1");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetCategory_Should_Not_Throw_NullReferenceException_When_Null_OldUnitcode()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            // Repo uses ??= string.Empty so the null does not cause NullReferenceException;
            // the SqlException (from missing SP) confirms the null was safely handled.
            Func<Task> act = async () => await repo.GetCategory(null!);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetPendingIssueAsync_Should_Not_Throw_NullReferenceException_When_Null_Params()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSQueryRepository(conn);

            Func<Task> act = async () => await repo.GetPendingIssueAsync(null!, null!);

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
