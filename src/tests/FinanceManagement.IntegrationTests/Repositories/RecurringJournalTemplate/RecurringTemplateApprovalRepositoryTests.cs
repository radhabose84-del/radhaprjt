using Contracts.Interfaces;
using Moq;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate;
using FinanceManagement.IntegrationTests.Common;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.RecurringJournalTemplate
{
    [Collection("DatabaseCollection")]
    public sealed class RecurringTemplateApprovalRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RecurringTemplateApprovalRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<(int PendingId, int ApprovedId)> SeedApprovalStatusAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = new MiscTypeMaster { MiscTypeCode = "ApprovalStatus", Description = "Approval status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.Add(type);
            await ctx.SaveChangesAsync();

            var pending = new MiscMaster { MiscTypeId = type.Id, Code = "Pending", Description = "Pending", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var approved = new MiscMaster { MiscTypeId = type.Id, Code = "Approved", Description = "Approved", SortOrder = 2, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.AddRange(pending, approved);
            await ctx.SaveChangesAsync();
            return (pending.Id, approved.Id);
        }

        [Fact]
        public async Task StatusId_FlowsThrough_Resolve_Pending_GetById_Schedule_And_Approve()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            var (pendingId, approvedId) = await SeedApprovalStatusAsync();

            // A pending template (AutoPost + !LowRisk would be the producer; here we just set Pending directly).
            int templateId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var template = RecurringTemplateSeed.BuildTemplate(ids);
                template.LowRisk = false;
                template.StatusId = pendingId;
                templateId = await new RecurringJournalTemplateCommandRepository(ctx).CreateAsync(template);
            }

            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(s => s.GetCompanyId()).Returns(1);
            var query = new RecurringJournalTemplateQueryRepository(new Microsoft.Data.SqlClient.SqlConnection(_fixture.ConnectionString), ipMock.Object);

            (await query.GetApprovalStatusIdAsync("Pending")).Should().Be(pendingId);
            (await query.GetApprovalStatusIdAsync("Approved")).Should().Be(approvedId);

            var byId = await query.GetByIdAsync(templateId);
            byId.Should().NotBeNull();
            byId!.StatusId.Should().Be(pendingId);
            byId.StatusName.Should().Be("Pending");

            var (pendingList, pendingTotal) = await query.GetPendingApprovalAsync(1, 50);
            pendingTotal.Should().Be(1);
            pendingList.Should().ContainSingle().Which.Id.Should().Be(templateId);

            var sched = await query.GetScheduleInfoAsync(templateId);
            sched.Should().NotBeNull();
            sched!.FrequencyCode.Should().Be("MONTHLY");
            sched.StatusCode.Should().Be("Pending");
            sched.AutoPost.Should().BeTrue();
            sched.LowRisk.Should().BeFalse();

            // Approve → status flips; no longer pending.
            await using (var ctx = _fixture.CreateFreshDbContext())
                (await new RecurringJournalTemplateCommandRepository(ctx).SetApprovalResultAsync(templateId, approvedId, CancellationToken.None))
                    .Should().BeTrue();

            (await query.GetByIdAsync(templateId))!.StatusName.Should().Be("Approved");
            (await query.GetPendingApprovalAsync(1, 50)).Item2.Should().Be(0);
            (await query.GetScheduleInfoAsync(templateId))!.StatusCode.Should().Be("Approved");
        }
    }
}
