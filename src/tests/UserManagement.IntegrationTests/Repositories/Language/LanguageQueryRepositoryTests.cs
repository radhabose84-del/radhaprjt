using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Language;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Language
{
    [Collection("DatabaseCollection")]
    public sealed class LanguageQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LanguageQueryRepositoryTests(DbFixture fixture)
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

        private LanguageQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new LanguageQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string code = "EN", string name = "English")
        {
            await using var ctx = CreateDbContext();
            var repo = new LanguageCommandRepository(ctx);
            var created = await repo.CreateAsync(new Domain.Entities.Language
            {
                Code = code,
                Name = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task ClearTableAsync()
        {
            await using var ctx = CreateDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Language");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllLanguageAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllLanguageAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllLanguageAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Language { IsDeleted = Enums.IsDelete.Deleted };
            await new LanguageCommandRepository(ctx).DeleteAsync(id, deleteModel);

            var (items, total) = await CreateQueryRepo().GetAllLanguageAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllLanguageAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("EN", "English");
            await SeedEntityAsync("FR", "French");

            var (items, total) = await CreateQueryRepo().GetAllLanguageAsync(1, 10, "French");

            items.Should().HaveCount(1);
            items[0].Name.Should().Be("French");
        }

        [Fact]
        public async Task GetAllLanguageAsync_Should_Return_Correct_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("EN", "English");
            await SeedEntityAsync("FR", "French");
            await SeedEntityAsync("DE", "German");

            var (items, total) = await CreateQueryRepo().GetAllLanguageAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("EN", "English");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("EN");
            result.Name.Should().Be("English");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Language { IsDeleted = Enums.IsDelete.Deleted };
            await new LanguageCommandRepository(ctx).DeleteAsync(id, deleteModel);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- GET BY LANGUAGE NAME ---

        [Fact]
        public async Task GetByLanguagenameAsync_Should_Return_Matching_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync("EN", "English");

            var result = await CreateQueryRepo().GetByLanguagenameAsync("English");

            result.Should().NotBeNull();
            result!.Name.Should().Be("English");
        }

        [Fact]
        public async Task GetByLanguagenameAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("EN", "English");

            var result = await CreateQueryRepo().GetByLanguagenameAsync("English", id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByLanguagenameAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByLanguagenameAsync("NonExistent");

            result.Should().BeNull();
        }

        // --- GET LANGUAGE (Autocomplete) ---

        [Fact]
        public async Task GetLanguage_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("EN", "English");
            await SeedEntityAsync("FR", "French");

            var results = await CreateQueryRepo().GetLanguage("Eng");

            results.Should().HaveCount(1);
        }
    }
}
