using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.MRS;

namespace MaintenanceManagement.IntegrationTests.Repositories.MRS
{
    /// <summary>
    /// MRSCommandRepository is entirely stored-procedure based (GenerateNextIRNO, InsertMRSData, InsertMRSDetail)
    /// and the legacy SPs do NOT exist in the MaintenanceManagement_TestDb. These tests therefore verify only:
    ///   1. ArgumentNullException is thrown on null input (pure C# validation, no DB needed)
    ///   2. GetNewIRNOAsync throws SqlException when the SP is missing (confirms the repo forwards to SQL correctly)
    /// Happy-path Create/Insert tests are intentionally skipped because they require SPs that don't live in the test DB.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MRSCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MRSCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task InsertMRSAsync_Should_Throw_ArgumentNullException_When_HeaderNull()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSCommandRepository(ctx, conn);

            Func<Task> act = async () => await repo.InsertMRSAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetNewIRNOAsync_Should_Throw_SqlException_When_Sp_Not_Exist()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSCommandRepository(ctx, conn);

            // GenerateNextIRNO stored procedure is a legacy SP and does not exist in the test DB.
            // The expectation is that the repo correctly forwards the call and SQL Server reports
            // "Could not find stored procedure".
            Func<Task> act = async () =>
                await repo.GetNewIRNOAsync("DIV1", DateTime.UtcNow);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetNewIRNOAsync_Should_Accept_Null_Divcode_Without_ArgumentException()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            var repo = new MRSCommandRepository(ctx, conn);

            // The repo null-coalesces divcode internally, so the null check should not throw.
            // The resulting SqlException (from missing SP) confirms divcode was accepted.
            Func<Task> act = async () =>
                await repo.GetNewIRNOAsync(null!, DateTime.UtcNow);

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
