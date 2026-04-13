using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers;

namespace MaintenanceManagement.IntegrationTests.Repositories.PreventiveSchedulers
{
    [Collection("DatabaseCollection")]
    public sealed class PreventiveSchedulerQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PreventiveSchedulerQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PreventiveSchedulerQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PreventiveSchedulerQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- CalculateNextScheduleDate (pure calculation — no DB) ---

        [Fact]
        public async Task CalculateNextScheduleDate_Days_Should_Return_Correct_Dates()
        {
            var startDate = new DateTime(2026, 1, 1);
            var repo = CreateQueryRepo();

            var (nextDate, reminderDate) = await repo.CalculateNextScheduleDate(startDate, 10, "days", 2);

            nextDate.Should().Be(startDate.AddDays(10));
            reminderDate.Should().Be(nextDate.AddDays(-2));
        }

        [Fact]
        public async Task CalculateNextScheduleDate_Months_Should_Return_Correct_Dates()
        {
            var startDate = new DateTime(2026, 1, 15);
            var repo = CreateQueryRepo();

            var (nextDate, reminderDate) = await repo.CalculateNextScheduleDate(startDate, 3, "months", 5);

            nextDate.Should().Be(startDate.AddMonths(3));
            reminderDate.Should().Be(nextDate.AddDays(-5));
        }

        [Fact]
        public async Task CalculateNextScheduleDate_Years_Should_Return_Correct_Dates()
        {
            var startDate = new DateTime(2026, 6, 1);
            var repo = CreateQueryRepo();

            var (nextDate, reminderDate) = await repo.CalculateNextScheduleDate(startDate, 1, "years", 10);

            nextDate.Should().Be(startDate.AddYears(1));
            reminderDate.Should().Be(nextDate.AddDays(-10));
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Id_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundDetailAsync_Should_Return_False_When_Id_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundDetailAsync(999999);

            result.Should().BeFalse();
        }

        // --- AlreadyExistsAsync ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Empty()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().AlreadyExistsAsync(1, 1);

            result.Should().BeFalse();
        }

        // --- UpdateValidation ---

        [Fact]
        public async Task UpdateValidation_Should_Return_False_When_Empty()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().UpdateValidation(999999);

            result.Should().BeFalse();
        }
    }
}
