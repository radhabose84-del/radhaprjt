using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Divisions;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Division
{
    [Collection("DatabaseCollection")]
    public sealed class DivisionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DivisionCommandRepositoryTests(DbFixture fixture)
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

        private DivisionCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new DivisionCommandRepository(ctx);

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

        private UserManagement.Domain.Entities.Division BuildDivision(
            int companyId,
            string shortName = "TST",
            string name = "Test Division")
        {
            return new UserManagement.Domain.Entities.Division
            {
                ShortName = shortName,
                Name = name,
                CompanyId = companyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
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

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var repo = CreateRepository(ctx);
            var division = BuildDivision(companyId);

            var result = await repo.CreateAsync(division);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var repo = CreateRepository(ctx);
            var division = BuildDivision(companyId, "MFG", "Manufacturing");

            var result = await repo.CreateAsync(division);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Divisions.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.ShortName.Should().Be("MFG");
            saved.Name.Should().Be("Manufacturing");
            saved.CompanyId.Should().Be(companyId);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var repo = CreateRepository(ctx);
            var division = BuildDivision(companyId);

            var result = await repo.CreateAsync(division);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Divisions.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
            saved.CreatedByName.Should().NotBeNullOrEmpty();
            saved.CreatedIP.Should().NotBeNullOrEmpty();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_And_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var repo = CreateRepository(ctx);
            var division = BuildDivision(companyId, "OLD", "Old Division");
            var created = await repo.CreateAsync(division);
            ctx.ChangeTracker.Clear();

            var updateDiv = new UserManagement.Domain.Entities.Division
            {
                Id = created.Id,
                ShortName = "NEW",
                Name = "New Division",
                CompanyId = companyId,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(updateDiv);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.Divisions.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.ShortName.Should().Be("NEW");
            updated.Name.Should().Be("New Division");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var updateDiv = new UserManagement.Domain.Entities.Division
            {
                Id = 99999,
                ShortName = "X",
                Name = "X",
                CompanyId = 1,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(updateDiv);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Change_IsActive_Status()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var repo = CreateRepository(ctx);
            var division = BuildDivision(companyId, "STS", "Status Division");
            var created = await repo.CreateAsync(division);
            ctx.ChangeTracker.Clear();

            var updateDiv = new UserManagement.Domain.Entities.Division
            {
                Id = created.Id,
                ShortName = "STS",
                Name = "Status Division",
                CompanyId = companyId,
                IsActive = Enums.Status.Inactive
            };

            var result = await repo.UpdateAsync(updateDiv);
            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.Divisions.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated!.IsActive.Should().Be(Enums.Status.Inactive);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Division()
        {
            await using var ctx = CreateDbContext();
            await ClearDivisionsAsync(ctx);
            var companyId = await EnsureCompanyAsync(ctx);

            var repo = CreateRepository(ctx);
            var division = BuildDivision(companyId);
            var created = await repo.CreateAsync(division);
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserManagement.Domain.Entities.Division
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.Divisions.FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var deleteModel = new UserManagement.Domain.Entities.Division
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(99999, deleteModel);

            result.Should().BeFalse();
        }
    }
}
