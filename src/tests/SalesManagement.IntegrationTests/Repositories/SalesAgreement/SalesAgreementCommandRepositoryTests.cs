using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesAgreement;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesAgreement
{
    [Collection("DatabaseCollection")]
    public sealed class SalesAgreementCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesAgreementCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // IncrementDocNoAsync no-op (DocumentSequence side effect out of scope).
        private SalesAgreementCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                  .Returns(Task.CompletedTask);
            return new SalesAgreementCommandRepository(ctx, docSeq.Object);
        }

        // StatusId → Sales.MiscMaster (FK, must be under MiscType 'ApprovalStatus' for StatusExists checks),
        // SalesGroupId → Sales.SalesGroup (FK via SalesOrganisation → SalesOffice → SalesGroup chain).
        private async Task<(int sgId, int statusId, int cancelledId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "ApprovalStatus",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            var statusId = await EnsureMiscAsync(ctx, mt.Id, "Pending");
            var cancelledId = await EnsureMiscAsync(ctx, mt.Id, "Cancelled");

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SAGORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SAGORG", SalesOrganisationName = "SAG Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SAG_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SAG_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "SAG_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "SAG_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            return (sg.Id, statusId, cancelledId);
        }

        private static async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private static SalesManagement.Domain.Entities.SalesAgreementHeader BuildEntity(
            int sgId, int statusId, string agreementNo) =>
            new()
            {
                AgreementNo = agreementNo,
                StatusId = statusId,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 12, 31),
                CustomerId = 100,
                SalesGroupId = sgId,
                PaymentTermsId = 1,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                SalesAgreementDetails = new List<SalesManagement.Domain.Entities.SalesAgreementDetail>
                {
                    new() { ItemId = 10, AgreedRate = 50m, TotalQty = 100m, ReleasedQty = 0m }
                }
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            var (sgId, statusId, _) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(sgId, statusId, "SAG_C1"), 1);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Detail()
        {
            await ClearAsync();
            var (sgId, statusId, _) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(sgId, statusId, "SAG_C2"), 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesAgreementHeader.FirstAsync(x => x.Id == id);
            saved.AgreementNo.Should().Be("SAG_C2");
            saved.SalesGroupId.Should().Be(sgId);
            saved.StatusId.Should().Be(statusId);

            var detailCount = await ctx.SalesAgreementDetail.CountAsync(d => d.SalesAgreementHeaderId == id);
            detailCount.Should().Be(1);
        }

        [Fact]
        public async Task CancelAsync_Should_Set_Status_And_Return_True()
        {
            await ClearAsync();
            var (sgId, statusId, cancelledId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(sgId, statusId, "SAG_CX"), 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).CancelAsync(id, cancelledId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.SalesAgreementHeader.FirstAsync(x => x.Id == id);
            reloaded.StatusId.Should().Be(cancelledId);
        }

        [Fact]
        public async Task CancelAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).CancelAsync(9999999, 1, CancellationToken.None);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAgentPOAttachmentAsync_Should_Persist_FileName()
        {
            await ClearAsync();
            var (sgId, statusId, _) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(sgId, statusId, "SAG_DOC"), 1);
            ctx.ChangeTracker.Clear();

            var ok = await CreateRepo(ctx).UpdateAgentPOAttachmentAsync(id, "agreement-1.pdf", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            ok.Should().BeTrue();
            var reloaded = await ctx.SalesAgreementHeader.FirstAsync(x => x.Id == id);
            reloaded.AgentPOAttachment.Should().Be("agreement-1.pdf");
        }

        [Fact]
        public async Task UpdateAgentPOAttachmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx).UpdateAgentPOAttachmentAsync(9999999, "x.pdf", CancellationToken.None);
            ok.Should().BeFalse();
        }
    }
}
