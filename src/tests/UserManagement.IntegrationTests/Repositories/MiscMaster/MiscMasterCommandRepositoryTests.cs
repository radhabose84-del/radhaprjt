using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.MiscMaster;
using UserManagement.Infrastructure.Repositories.MiscTypeMaster;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
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

        private MiscMasterCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new MiscMasterCommandRepository(ctx);

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "MM_TEST_TYPE");
            if (existing != null) return existing.Id;

            var miscType = new UserManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "MM_TEST_TYPE",
                Description = "MiscMaster Test Type",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return miscType.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx, int miscTypeId) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = "MMC_01",
                Description = "Test MiscMaster CMD",
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await repo.CreateAsync(entity);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = "MMC_02",
                Description = "Alpha MiscMaster",
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("MMC_02");
            saved.Description.Should().Be("Alpha MiscMaster");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = "MMC_03",
                Description = "Audit Test",
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = "MMC_04",
                Description = "Old Description",
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var created = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.MiscMaster
            {
                Id = created.Id,
                MiscTypeId = miscTypeId,
                Code = "MMC_04",
                Description = "Updated Description",
                SortOrder = 2,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(created.Id, updateEntity);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Updated Description");
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = "MMC_05",
                Description = "Delete MiscMaster",
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var created = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.MiscMaster { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }
    }
}
