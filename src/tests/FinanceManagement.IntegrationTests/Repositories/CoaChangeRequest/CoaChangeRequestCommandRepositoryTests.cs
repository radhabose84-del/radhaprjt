using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.CoaChangeRequest;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.CoaChangeRequest
{
    // US-GL02-08B — change-request / unfreeze write side, incl. the G2 auto-capture transition,
    // against the real test DB (EnsureCreated builds the two new tables from the model).
    [Collection("DatabaseCollection")]
    public sealed class CoaChangeRequestCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CoaChangeRequestCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private static CoaChangeRequestCommandRepository Repo(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Finance.CoaChangeRequest");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Finance.CoaUnfreezeRequest");
        }

        private static Domain.Entities.CoaChangeRequest NewChangeRequest(int companyId = 1, string status = CoaChangeRequestStatus.ImpactApproved, int? accountId = null) =>
            new()
            {
                CompanyId = companyId,
                TargetAccountId = accountId,
                ChangeType = CoaChangeType.AccountEdit,
                Justification = "year-end correction",
                ImpactAssessment = "no downstream impact",
                RequestStatus = status,
                RequestedByUserId = 3
            };

        [Fact]
        public async Task GetImpactApprovedChangeRequests_ReturnsOnlyImpactApprovedForCompany()
        {
            await ClearAsync();
            int id1, id2;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var approved = NewChangeRequest(status: CoaChangeRequestStatus.ImpactApproved);
                var pending = NewChangeRequest(status: CoaChangeRequestStatus.PendingImpactApproval);
                var otherCo = NewChangeRequest(companyId: 99, status: CoaChangeRequestStatus.ImpactApproved);
                ctx.CoaChangeRequest.AddRange(approved, pending, otherCo);
                await ctx.SaveChangesAsync();
                id1 = approved.Id; id2 = pending.Id;
            }

            await using var read = _fixture.CreateFreshDbContext();
            var result = await Repo(read).GetImpactApprovedChangeRequestsAsync(new[] { id1, id2 }, 1, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }

        // G2 — while a window is open, a structural write commits a pending change request as Post-Freeze.
        [Fact]
        public async Task TryCapturePostFreezeChange_CommitsChangeRequest_WhenWindowOpen()
        {
            await ClearAsync();
            int windowId, crId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var window = new CoaUnfreezeRequest
                {
                    CompanyId = 1,
                    Reason = "year-end",
                    RequestStatus = CoaUnfreezeRequestStatus.WindowOpen,
                    WindowMinutes = 60,
                    WindowExpiry = DateTimeOffset.UtcNow.AddMinutes(30),
                    RequestedByUserId = 3
                };
                ctx.CoaUnfreezeRequest.Add(window);
                await ctx.SaveChangesAsync();
                windowId = window.Id;

                var cr = NewChangeRequest(accountId: 555);
                cr.UnfreezeRequestId = windowId;
                ctx.CoaChangeRequest.Add(cr);
                await ctx.SaveChangesAsync();
                crId = cr.Id;
            }

            bool captured;
            await using (var ctx = _fixture.CreateFreshDbContext())
                captured = await Repo(ctx).TryCapturePostFreezeChangeAsync(
                    companyId: 1, accountId: 555, accountGroupId: null, userId: 9, now: DateTimeOffset.UtcNow, CancellationToken.None);

            captured.Should().BeTrue();

            await using var read = _fixture.CreateFreshDbContext();
            var committed = await read.CoaChangeRequest.FirstAsync(x => x.Id == crId);
            committed.RequestStatus.Should().Be(CoaChangeRequestStatus.Committed);
            committed.IsPostFreeze.Should().BeTrue();
            committed.CommittedByUserId.Should().Be(9);
        }

        [Fact]
        public async Task TryCapturePostFreezeChange_ReturnsFalse_WhenNoOpenWindow()
        {
            await ClearAsync();
            // An impact-approved request exists but no open window → nothing to capture.
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.CoaChangeRequest.Add(NewChangeRequest());
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var captured = await Repo(ctx2).TryCapturePostFreezeChangeAsync(1, 555, null, 9, DateTimeOffset.UtcNow, CancellationToken.None);

            captured.Should().BeFalse();
        }
    }
}
