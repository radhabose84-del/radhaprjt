using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.LogServices;

namespace PurchaseManagement.IntegrationTests.Repositories.LogServices
{
    [Collection("DatabaseCollection")]
    public sealed class LogServiceCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LogServiceCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private LogServiceCommandRepository CreateRepo(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static PurchaseManagement.Domain.Entities.IndentLog BuildLog(
            int indentHeaderId = 1,
            string actionType = "Create",
            string actionRemarks = "Created indent",
            int statusId = 1) =>
            new()
            {
                IndentHeaderId = indentHeaderId,
                ActionType = actionType,
                ActionRemarks = actionRemarks,
                PreviousData = "{}",
                NewData = "{}",
                StatusId = statusId
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_True_On_Success()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).CreateAsync(BuildLog());

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Log_With_GeneratedId()
        {
            await _fixture.ClearAllTablesAsync();
            var log = BuildLog(indentHeaderId: 42, actionType: "Update");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(log);

            log.Id.Should().BeGreaterThan(0);

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var saved = await verifyCtx.IndentLog.FirstOrDefaultAsync(x => x.Id == log.Id);
            saved.Should().NotBeNull();
            saved!.IndentHeaderId.Should().Be(42);
            saved.ActionType.Should().Be("Update");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields_From_IpService()
        {
            await _fixture.ClearAllTablesAsync();
            var log = BuildLog();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(log);

            log.CreatedBy.Should().Be(1);             // from IpMock.Setup(GetUserId).Returns(1)
            log.CreatedByName.Should().Be("test-user");
            log.CreatedIP.Should().Be("127.0.0.1");
            log.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Multiple_Logs_Independently()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            await repo.CreateAsync(BuildLog(indentHeaderId: 100, actionType: "Create"));
            await repo.CreateAsync(BuildLog(indentHeaderId: 100, actionType: "Update"));
            await repo.CreateAsync(BuildLog(indentHeaderId: 100, actionType: "Delete"));

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var saved = await verifyCtx.IndentLog
                .Where(x => x.IndentHeaderId == 100)
                .OrderBy(x => x.Id)
                .ToListAsync();

            saved.Should().HaveCount(3);
            saved.Select(x => x.ActionType).Should().ContainInOrder("Create", "Update", "Delete");
        }
    }
}
