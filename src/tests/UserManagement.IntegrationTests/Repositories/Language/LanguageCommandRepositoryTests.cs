using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Language;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Language
{
    [Collection("DatabaseCollection")]
    public sealed class LanguageCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LanguageCommandRepositoryTests(DbFixture fixture)
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

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private static LanguageCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.Language BuildEntity(
            string code = "EN",
            string name = "English") =>
            new()
            {
                Code = code,
                Name = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("FR", "French"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Languages.FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("FR");
            saved.Name.Should().Be("French");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Languages.FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildEntity(code: "EN", name: "Updated English");
            updateEntity.Id = created.Id;

            var result = await CreateRepository(ctx).UpdateAsync(updateEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildEntity(code: "EN2", name: "Updated English");
            updateEntity.Id = created.Id;
            await CreateRepository(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.Languages.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated.Should().NotBeNull();
            updated!.Code.Should().Be("EN2");
            updated.Name.Should().Be("Updated English");
        }

        [Fact]
        public async Task UpdateAsync_NonExistent_Should_Return_False()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var updateEntity = BuildEntity(name: "Ghost");
            updateEntity.Id = 9999;

            var result = await CreateRepository(ctx).UpdateAsync(updateEntity);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Language
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(created.Id, deleteModel);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Language
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            await CreateRepository(ctx).DeleteAsync(created.Id, deleteModel);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Languages.FirstOrDefaultAsync(x => x.Id == created.Id);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_NonExistent_Should_Return_False()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var deleteModel = new Domain.Entities.Language
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteModel);

            result.Should().BeFalse();
        }
    }
}
