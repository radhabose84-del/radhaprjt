using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Moq;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesLead;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesLead
{
    [Collection("DatabaseCollection")]
    public sealed class SalesLeadCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup;

        public SalesLeadCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _mockDocSeqLookup = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            _mockDocSeqLookup
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>()))
                .Returns(Task.CompletedTask);
        }

        private SalesLeadCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx, _mockDocSeqLookup.Object);

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.EmployeeNo == "SLC_MO");
            if (existing != null) return existing.Id;

            // Seed prerequisite chain: SalesOrganisation -> SalesOffice -> MarketingOfficer
            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SLC_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SLC_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SLC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SLC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var mo = new SalesManagement.Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "SLC_MO",
                EmployeeName = "Officer Test",
                MobileNo = "9876543210",
                Email = "mo@y.com",
                Unit = "U", Department = "Sales", Designation = "Mgr",
                SalesOfficeId = office.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MarketingOfficer.AddAsync(mo);
            await ctx.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<SalesManagement.Domain.Entities.SalesLead> BuildEntityAsync(string company, string? leadNo = null)
        {
            var moId = await EnsureMarketingOfficerAsync();
            return new SalesManagement.Domain.Entities.SalesLead
            {
                LeadNo = leadNo ?? $"SL-{company}",
                ProspectCompanyName = company,
                ContactName = "John",
                MobileNumber = "9876543210",
                EmailId = "x@y.com",
                MarketingOfficerId = moId,
                InteractionDate = DateTimeOffset.UtcNow,
                Remarks = "test lead",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SL_C1"), 1);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SL_C2"), 1);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.SalesLead.FirstAsync(x => x.Id == id);

            saved.ProspectCompanyName.Should().Be("SL_C2");
            saved.ContactName.Should().Be("John");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SL_U1"), 1);
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("SL_U1_New");
            entity.Id = id;
            entity.ContactName = "Jane";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.SalesLead.FirstAsync(x => x.Id == id);
            reloaded.ProspectCompanyName.Should().Be("SL_U1_New");
            reloaded.ContactName.Should().Be("Jane");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("GH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SL_D1"), 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SL_D2"), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.SalesLead.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_UomId_Change()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var initial = await BuildEntityAsync("SL_UOM");
            initial.UomId = 5;
            var id = await CreateRepo(ctx).CreateAsync(initial, 1);
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("SL_UOM");
            updated.Id = id;
            updated.UomId = 7;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.SalesLead.FirstAsync(x => x.Id == id);
            reloaded.UomId.Should().Be(7);
        }
    }
}
