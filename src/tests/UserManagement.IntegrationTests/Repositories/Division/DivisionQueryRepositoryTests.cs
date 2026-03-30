using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Divisions;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Division
{
    [Collection("DatabaseCollection")]
    public sealed class DivisionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DivisionQueryRepositoryTests(DbFixture fixture)
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

        private DivisionQueryRepository CreateQueryRepo(int companyId = 1)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(companyId);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DivisionQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> EnsureCompanyAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.Companies.FirstOrDefaultAsync();
            if (existing != null)
                return existing.Id;

            var company = new Company
            {
                CompanyName = "Test Company",
                LegalName = "Test Company Pvt Ltd",
                GstNumber = "GSTTEST1234",
                TIN = "TIN1234",
                TAN = "TAN1234",
                CSTNo = "CST1234",
                YearOfEstablishment = 2020,
                Website = "https://example.com",
                EntityId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted,
                PanNumber = "PAN1234",
                CompanyAddress = new CompanyAddress
                {
                    AddressLine1 = "Line 1",
                    AddressLine2 = "Line 2",
                    PinCode = "600001",
                    AlternatePhone = "9876543210",
                    Phone = "0123456789",
                    CountryId = 1,
                    StateId = 1,
                    CityId = 1
                },
                CompanyContact = new CompanyContact
                {
                    Name = "Contact Person",
                    Designation = "Manager",
                    Email = "contact@example.com",
                    Phone = "9999999999",
                    Remarks = "Primary contact"
                }
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();
            return company.Id;
        }

        private async Task<int> SeedDivisionAsync(
            ApplicationDbContext ctx,
            int companyId,
            string shortName = "TST",
            string name = "Test Division",
            Enums.Status isActive = Enums.Status.Active)
        {
            var division = new UserManagement.Domain.Entities.Division
            {
                ShortName = shortName,
                Name = name,
                CompanyId = companyId,
                IsActive = isActive,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var repo = new DivisionCommandRepository(ctx);
            var created = await repo.CreateAsync(division);
            ctx.ChangeTracker.Clear();
            return created.Id;
        }

        private async Task ClearDivisionsAsync(ApplicationDbContext ctx)
        {
            // Clear Units first (FK dependency on Division)
            var units = await ctx.Unit.ToListAsync();
            ctx.Unit.RemoveRange(units);
            await ctx.SaveChangesAsync();

            var divisions = await ctx.Divisions.ToListAsync();
            ctx.Divisions.RemoveRange(divisions);
            await ctx.SaveChangesAsync();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllDivisionAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            await SeedDivisionAsync(ctx, companyId, "A01", "Alpha Division");
            await SeedDivisionAsync(ctx, companyId, "B02", "Beta Division");

            var repo = CreateQueryRepo(companyId);
            var (items, total) = await repo.GetAllDivisionAsync(1, 10, null);

            items.Should().HaveCount(2);
            total.Should().Be(2);
        }

        [Fact]
        public async Task GetAllDivisionAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            await SeedDivisionAsync(ctx, companyId, "A01", "Alpha Division");
            await SeedDivisionAsync(ctx, companyId, "B02", "Beta Division");

            var repo = CreateQueryRepo(companyId);
            var (items, total) = await repo.GetAllDivisionAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Alpha Division");
        }

        [Fact]
        public async Task GetAllDivisionAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "DEL", "Deleted Division");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new DivisionCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new UserManagement.Domain.Entities.Division
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo(companyId);
            var (items, total) = await repo.GetAllDivisionAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Division()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "BID", "ById Division");

            var repo = CreateQueryRepo(companyId);
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ShortName.Should().Be("BID");
            result.Name.Should().Be("ById Division");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "DEL", "Deleted Division");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new DivisionCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new UserManagement.Domain.Entities.Division
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo(companyId);
            var result = await repo.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent_Id()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(99999);

            result.Should().BeNull();
        }

        // --- GET BY DIVISION NAME ---

        [Fact]
        public async Task GetByDivisionnameAsync_Should_Return_Matching_Division()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            await SeedDivisionAsync(ctx, companyId, "UNQ", "Unique Division");

            var repo = CreateQueryRepo(companyId);
            var result = await repo.GetByDivisionnameAsync("Unique Division");

            result.Should().NotBeNull();
            result!.Name.Should().Be("Unique Division");
        }

        [Fact]
        public async Task GetByDivisionnameAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "SLF", "Self Division");

            var repo = CreateQueryRepo(companyId);
            var result = await repo.GetByDivisionnameAsync("Self Division", id);

            result.Should().BeNull();
        }

        // --- FK COLUMN EXIST VALIDATION ---

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Division()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "ACT", "Active Division", Enums.Status.Active);

            var repo = CreateQueryRepo(companyId);
            var result = await repo.FKColumnExistValidation(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_Inactive_Division()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "INA", "Inactive Division", Enums.Status.Inactive);

            var repo = CreateQueryRepo(companyId);
            var result = await repo.FKColumnExistValidation(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.FKColumnExistValidation(99999);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_Units_Linked()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var id = await SeedDivisionAsync(ctx, companyId, "NLK", "No Link Division");

            var repo = CreateQueryRepo(companyId);
            var result = await repo.SoftDeleteValidation(id);

            result.Should().BeFalse();
        }
    }
}
