using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.FinancialYear;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.FinancialYear
{
    [Collection("DatabaseCollection")]
    public sealed class FinancialYearCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FinancialYearCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private FinancialYearCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new FinancialYearCommandRepository(ctx);

        // FinancialYear.StatusId is a required FK to AppData.MiscMaster (US-GL03-01 FYS lifecycle).
        // Seeded per-test (ClearAllTablesAsync wipes AppData) and captured here for BuildEntity.
        private int _statusId;

        private Domain.Entities.FinancialYear BuildEntity(
            string startYear = "2024",
            string finYearName = "FY-CMD-TEST") =>
            new Domain.Entities.FinancialYear
            {
                StartYear = startYear,
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2025, 3, 31),
                FinYearName = finYearName,
                StatusId = _statusId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        // Seeds the 'FYS' MiscType + an 'OPEN' MiscMaster status so FinancialYear's required
        // StatusId FK is satisfied, returning the status id.
        private static async Task<int> EnsureFysStatusAsync(ApplicationDbContext ctx)
        {
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "FYS");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "FYS",
                    Description = "Financial Year Status",
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            var status = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "OPEN" && m.MiscTypeId == type.Id);
            if (status == null)
            {
                status = new Domain.Entities.MiscMaster
                {
                    Code = "OPEN",
                    Description = "Open",
                    MiscTypeId = type.Id,
                    SortOrder = 1,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(status);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            return status.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();
            _statusId = await EnsureFysStatusAsync(ctx);
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildEntity());

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildEntity("2025", "FY-CMD-ALPHA"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FinancialYear.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.StartYear.Should().Be("2025");
            saved.FinYearName.Should().Be("FY-CMD-ALPHA");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FinancialYear.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildEntity("2024", "FY-CMD-UPDATE"));
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.FinancialYear
            {
                StartYear = "2025",
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2026, 3, 31),
                FinYearName = "FY-CMD-UPDATED",
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(created.Id, updateEntity);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.FinancialYear.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.FinYearName.Should().Be("FY-CMD-UPDATED");
            updated.StartYear.Should().Be("2025");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(99999, BuildEntity());

            result.Should().Be(-1);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildEntity("2024", "FY-CMD-DEL"));
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.FinancialYear
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.FinancialYear.FirstOrDefaultAsync(x => x.Id == created.Id);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.DeleteAsync(99999, new Domain.Entities.FinancialYear { IsDeleted = Enums.IsDelete.Deleted });

            result.Should().Be(0);
        }
    }
}
