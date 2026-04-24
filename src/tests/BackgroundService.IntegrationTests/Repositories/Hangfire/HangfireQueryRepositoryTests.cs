using BackgroundService.Infrastructure.Repositories.HangFire;
using Dapper;
using Microsoft.Data.SqlClient;

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

        private HangfireQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task EnsureHangfireSchemaAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HangFire')
                    EXEC('CREATE SCHEMA [HangFire]');");
            await conn.ExecuteAsync(@"
                IF OBJECT_ID('HangFire.Job') IS NULL
                BEGIN
                    CREATE TABLE HangFire.Job(
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        StateId int NULL,
                        StateName nvarchar(50) NULL,
                        InvocationData nvarchar(max) NULL,
                        Arguments nvarchar(max) NULL,
                        CreatedAt datetime NOT NULL DEFAULT SYSUTCDATETIME(),
                        ExpireAt datetime NULL);
                END");
            await conn.ExecuteAsync(@"
                IF OBJECT_ID('HangFire.State') IS NULL
                BEGIN
                    CREATE TABLE HangFire.[State](
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        JobId int NOT NULL,
                        Name nvarchar(50) NOT NULL,
                        Reason nvarchar(100) NULL,
                        CreatedAt datetime NOT NULL DEFAULT SYSUTCDATETIME(),
                        Data nvarchar(max) NULL);
                END");
        }

        private async Task ClearHangfireAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                IF OBJECT_ID('HangFire.State') IS NOT NULL DELETE FROM HangFire.[State];
                IF OBJECT_ID('HangFire.Job')   IS NOT NULL DELETE FROM HangFire.Job;");
        }

        /// <summary>
        /// Inserts a Job row and matching State row, returning the JobId.
        /// The Arguments column is a JSON array whose first element must be the
        /// transactionId the repository's TRY_CONVERT + JSON_VALUE expression filters on.
        /// </summary>
        private async Task<int> SeedJobAsync(int transactionArg, string stateName)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var jobId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO HangFire.Job (Arguments)
                OUTPUT INSERTED.Id
                VALUES (@args);",
                new { args = $"[\"{transactionArg}\"]" });
            var stateId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO HangFire.[State] (JobId, Name)
                OUTPUT INSERTED.Id
                VALUES (@jobId, @name);",
                new { jobId, name = stateName });
            await conn.ExecuteAsync(@"
                UPDATE HangFire.Job SET StateId = @stateId WHERE Id = @jobId;",
                new { stateId, jobId });
            return jobId;
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Returns_Empty_When_No_Jobs()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();

            var result = await CreateRepo().GetHangfireJobByTransactionId(42);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Returns_Scheduled_Job()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            var jobId = await SeedJobAsync(100, "Scheduled");

            var result = await CreateRepo().GetHangfireJobByTransactionId(100);

            result.Should().ContainSingle().Which.Should().Be(jobId);
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Returns_Enqueued_Job()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            var jobId = await SeedJobAsync(200, "Enqueued");

            var result = await CreateRepo().GetHangfireJobByTransactionId(200);

            result.Should().ContainSingle().Which.Should().Be(jobId);
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Returns_Processing_Job()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            var jobId = await SeedJobAsync(300, "Processing");

            var result = await CreateRepo().GetHangfireJobByTransactionId(300);

            result.Should().ContainSingle().Which.Should().Be(jobId);
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Excludes_Succeeded_Job()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            await SeedJobAsync(400, "Succeeded");

            var result = await CreateRepo().GetHangfireJobByTransactionId(400);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Excludes_Failed_Job()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            await SeedJobAsync(500, "Failed");

            var result = await CreateRepo().GetHangfireJobByTransactionId(500);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Filters_By_Transaction()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            await SeedJobAsync(600, "Enqueued");
            await SeedJobAsync(700, "Enqueued");

            var result = await CreateRepo().GetHangfireJobByTransactionId(600);

            result.Should().ContainSingle();
        }

        [Fact]
        public async Task GetHangfireJobByTransactionId_Returns_Multiple_Matching_Jobs()
        {
            await EnsureHangfireSchemaAsync();
            await ClearHangfireAsync();
            await SeedJobAsync(800, "Scheduled");
            await SeedJobAsync(800, "Enqueued");

            var result = await CreateRepo().GetHangfireJobByTransactionId(800);

            result.Should().HaveCount(2);
        }
    }
}
