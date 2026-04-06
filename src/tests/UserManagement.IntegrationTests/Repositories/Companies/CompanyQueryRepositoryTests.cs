using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Companies;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Companies
{
    [Collection("DatabaseCollection")]
    public sealed class CompanyQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanyQueryRepositoryTests(DbFixture fixture)
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

        private CompanyQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CompanyQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedEntityAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.Entity.FirstOrDefaultAsync(e => e.EntityName == "TestEntity_CQR");
            if (existing != null) return existing.Id;

            var entity = new UserManagement.Domain.Entities.Entity
            {
                EntityName = "TestEntity_CQR",
                EntityCode = "ENT-CQR01",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Entity.AddAsync(entity);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return entity.Id;
        }

        private async Task<int> SeedCompanyAsync(
            ApplicationDbContext ctx,
            int entityId,
            string companyName = "Test Company CQR",
            string legalName = "Legal Name CQR")
        {
            var existing = await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyName == companyName && c.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var company = new Company
            {
                CompanyName = companyName,
                LegalName = legalName,
                EntityId = entityId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return company.Id;
        }

        private async Task ClearTestCompaniesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE AppData.Company SET IsDeleted = 1 WHERE CompanyName LIKE 'Test Company CQR%'");
        }

        // --- GET BY COMPANY NAME ---

        [Fact]
        public async Task GetByCompanynameAsync_Should_Return_Company_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestCompaniesAsync(ctx);
            var entityId = await SeedEntityAsync(ctx);
            await SeedCompanyAsync(ctx, entityId, "Test Company CQR Alpha");

            var repo = CreateQueryRepo();
            var result = await repo.GetByCompanynameAsync("Test Company CQR Alpha");

            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("Test Company CQR Alpha");
        }

        [Fact]
        public async Task GetByCompanynameAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByCompanynameAsync("NonExistentCompanyXYZ999");

            result.Should().BeNull();
        }

        // --- COMPANY EXISTS ASYNC ---

        [Fact]
        public async Task CompanyExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestCompaniesAsync(ctx);
            var entityId = await SeedEntityAsync(ctx);
            await SeedCompanyAsync(ctx, entityId, "Test Company CQR Beta");

            var repo = CreateQueryRepo();
            var exists = await repo.CompanyExistsAsync("Test Company CQR Beta");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CompanyExistsAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var exists = await repo.CompanyExistsAsync("NonExistentCompany_XYZ_999");

            exists.Should().BeFalse();
        }
    }
}
