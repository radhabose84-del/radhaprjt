using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.CoaChangeRequest;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.CoaChangeRequest
{
    // US-GL02-08B — Dapper read side, incl. the AC3 post-freeze change-log read-model join.
    [Collection("DatabaseCollection")]
    public sealed class CoaChangeRequestQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CoaChangeRequestQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CoaChangeRequestQueryRepository QueryRepo() => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Finance.CoaChangeRequest");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Finance.CoaUnfreezeRequest");
        }

        [Fact]
        public async Task GetChangeRequests_FiltersByStatus()
        {
            await ClearAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.CoaChangeRequest.Add(new Domain.Entities.CoaChangeRequest
                {
                    CompanyId = 1, ChangeType = CoaChangeType.AccountEdit, Justification = "j", ImpactAssessment = "i",
                    RequestStatus = CoaChangeRequestStatus.ImpactApproved, RequestedByUserId = 3
                });
                ctx.CoaChangeRequest.Add(new Domain.Entities.CoaChangeRequest
                {
                    CompanyId = 1, ChangeType = CoaChangeType.AccountEdit, Justification = "j", ImpactAssessment = "i",
                    RequestStatus = CoaChangeRequestStatus.PendingImpactApproval, RequestedByUserId = 3
                });
                await ctx.SaveChangesAsync();
            }

            var (items, total) = await QueryRepo().GetChangeRequestsAsync(1, CoaChangeRequestStatus.ImpactApproved, 1, 10, CancellationToken.None);

            total.Should().Be(1);
            items.Should().OnlyContain(x => x.Status == CoaChangeRequestStatus.ImpactApproved);
        }

        // AC3 — committed post-freeze changes join their unfreeze window for both approvers + stamps.
        [Fact]
        public async Task GetPostFreezeChangeLog_ReturnsCommittedRowsWithBothApprovers()
        {
            await ClearAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var window = new CoaUnfreezeRequest
                {
                    CompanyId = 1, Reason = "year-end", RequestStatus = CoaUnfreezeRequestStatus.WindowOpen,
                    WindowMinutes = 60, RequestedByUserId = 3,
                    CfoApproverUserId = 7, CfoApprovedOn = DateTimeOffset.UtcNow,
                    SysAdminApproverUserId = 9, SysAdminApprovedOn = DateTimeOffset.UtcNow
                };
                ctx.CoaUnfreezeRequest.Add(window);
                await ctx.SaveChangesAsync();

                ctx.CoaChangeRequest.Add(new Domain.Entities.CoaChangeRequest
                {
                    CompanyId = 1, ChangeType = CoaChangeType.CodeChange, Justification = "fix code",
                    ImpactAssessment = "i", AccountCodeSnapshot = "1001",
                    RequestStatus = CoaChangeRequestStatus.Committed, IsPostFreeze = true,
                    CommittedByUserId = 9, CommittedOn = DateTimeOffset.UtcNow,
                    UnfreezeRequestId = window.Id, RequestedByUserId = 3
                });
                // A non-committed request must NOT appear in the log.
                ctx.CoaChangeRequest.Add(new Domain.Entities.CoaChangeRequest
                {
                    CompanyId = 1, ChangeType = CoaChangeType.AccountEdit, Justification = "pending",
                    ImpactAssessment = "i", RequestStatus = CoaChangeRequestStatus.ImpactApproved, RequestedByUserId = 3
                });
                await ctx.SaveChangesAsync();
            }

            var log = await QueryRepo().GetPostFreezeChangeLogAsync(1, CancellationToken.None);

            log.Should().HaveCount(1);
            var row = log[0];
            row.IsPostFreeze.Should().BeTrue();
            row.ChangeType.Should().Be(CoaChangeType.CodeChange);
            row.AccountCode.Should().Be("1001");
            row.CfoApproverUserId.Should().Be(7);
            row.SysAdminApproverUserId.Should().Be(9);
            row.Justification.Should().Be("fix code");
        }
    }
}
