using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.CustomFields;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.CustomFields
{
    [Collection("DatabaseCollection")]
    public sealed class CustomFieldQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CustomFieldQueryRepositoryTests(DbFixture fixture)
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

        private CustomFieldQuery CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CustomFieldQuery(conn);
        }

        private async Task<(int LabelTypeId, int DataTypeId)> EnsureMiscMasterAsync(ApplicationDbContext ctx)
        {
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "CF_TYPE");
            if (miscType == null)
            {
                miscType = new UserManagement.Domain.Entities.MiscTypeMaster { MiscTypeCode = "CF_TYPE", Description = "CF Type", IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            var labelType = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "LBLTYPE" && m.MiscTypeId == miscType.Id);
            if (labelType == null)
            {
                labelType = new UserManagement.Domain.Entities.MiscMaster { Code = "LBLTYPE", Description = "Label Type", MiscTypeId = miscType.Id, SortOrder = 1, IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted };
                await ctx.MiscMaster.AddAsync(labelType);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            var dataType = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "DATATYPE" && m.MiscTypeId == miscType.Id);
            if (dataType == null)
            {
                dataType = new UserManagement.Domain.Entities.MiscMaster { Code = "DATATYPE", Description = "Data Type", MiscTypeId = miscType.Id, SortOrder = 2, IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted };
                await ctx.MiscMaster.AddAsync(dataType);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            return (labelType.Id, dataType.Id);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string labelName = "TestCFQ_Field")
        {
            var (labelTypeId, dataTypeId) = await EnsureMiscMasterAsync(ctx);
            var cmdRepo = new CustomFieldCommand(ctx);
            var entity = new CustomField
            {
                LabelName = labelName,
                LabelTypeId = labelTypeId,
                DataTypeId = dataTypeId,
                Length = 100,
                IsRequired = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var id = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllCustomFieldsAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "TestCFQ_Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllCustomFieldsAsync(1, 100, null);

            items.Should().NotBeEmpty();
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllCustomFieldsAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "TestCFQ_Searchable");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllCustomFieldsAsync(1, 10, "TestCFQ_Searchable");

            items.Should().HaveCount(1);
            items[0].LabelName.Should().Be("TestCFQ_Searchable");
        }

        [Fact]
        public async Task GetAllCustomFieldsAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "TestCFQ_ToDelete");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new CustomFieldCommand(ctx2);
            await cmdRepo.DeleteAsync(id, new CustomField { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllCustomFieldsAsync(1, 100, "TestCFQ_ToDelete");

            items.Should().NotContain(x => x.Id == id);
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "TestCFQ_Exist");

            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync("TestCFQ_Exist");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync("NonExistentCF_XYZ999");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ASYNC ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "TestCFQ_Found");

            var repo = CreateQueryRepo();
            var found = await repo.NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var found = await repo.NotFoundAsync(99999);

            found.Should().BeFalse();
        }
    }
}
