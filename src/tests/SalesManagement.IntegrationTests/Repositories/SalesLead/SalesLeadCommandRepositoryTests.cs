using Microsoft.EntityFrameworkCore;
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

        public SalesLeadCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesLeadCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesLeadCommandRepository(ctx);

        private Domain.Entities.SalesLead BuildEntity(
            string contactName = "John Lead",
            string mobileNumber = "9876543210",
            int marketingOfficerId = 1,
            string? prospectCompanyName = "Prospect Corp",
            string? remarks = "Test Lead")
            => new Domain.Entities.SalesLead
            {
                PartyId = null,
                ProspectCompanyName = prospectCompanyName,
                CityId = null,
                ContactName = contactName,
                MobileNumber = mobileNumber,
                EmailId = "lead@test.com",
                ContactId = null,
                ItemId = null,
                RequirementQty = 100m,
                ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                Remarks = remarks,
                LeadSourceId = null,
                MarketingOfficerId = marketingOfficerId,
                InteractionDate = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            // SalesEnquiryHeader references SalesLead — clear dependents first
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesEnquiryDetail");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesEnquiryHeader");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesLead");
        }

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var existingId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn,
                "SELECT TOP 1 Id FROM Sales.MarketingOfficer WHERE IsDeleted = 0 AND IsActive = 1");
            if (existingId > 0) return existingId;

            var salesOfficeId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn,
                "SELECT TOP 1 Id FROM Sales.SalesOffice WHERE IsDeleted = 0");
            if (salesOfficeId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var so = new Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "Test Sales Office SL",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOffice.Add(so);
                await ctx.SaveChangesAsync();
                salesOfficeId = so.Id;
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mo = new Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "EMP_SL001",
                EmployeeName = "Test Officer SL",
                MobileNo = "9876543299",
                SalesOfficeId = salesOfficeId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx2.MarketingOfficer.Add(mo);
            await ctx2.SaveChangesAsync();
            return mo.Id;
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(marketingOfficerId: moId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var entity = BuildEntity(
                contactName: "Lead Person",
                mobileNumber: "9111111111",
                marketingOfficerId: moId,
                prospectCompanyName: "Prospect Inc",
                remarks: "High Value Lead");
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesLead.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ContactName.Should().Be("Lead Person");
            saved.MobileNumber.Should().Be("9111111111");
            saved.ProspectCompanyName.Should().Be("Prospect Inc");
            saved.Remarks.Should().Be("High Value Lead");
            saved.MarketingOfficerId.Should().Be(moId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(marketingOfficerId: moId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesLead.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(contactName: "Original Lead", marketingOfficerId: moId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(
                contactName: "Updated Lead",
                mobileNumber: "9222222222",
                marketingOfficerId: moId,
                remarks: "Updated Remarks");
            updated.Id = id;
            updated.IsActive = Status.Inactive;

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.SalesLead.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ContactName.Should().Be("Updated Lead");
            saved.MobileNumber.Should().Be("9222222222");
            saved.Remarks.Should().Be("Updated Remarks");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var updated = BuildEntity(marketingOfficerId: moId);
            updated.Id = 99999;

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        // ── SoftDeleteAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(marketingOfficerId: moId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(marketingOfficerId: moId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesLead
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(marketingOfficerId: moId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
