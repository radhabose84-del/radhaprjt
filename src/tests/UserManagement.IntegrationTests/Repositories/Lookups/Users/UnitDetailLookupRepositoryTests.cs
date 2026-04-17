using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class UnitDetailLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UnitDetailLookupRepositoryTests(DbFixture fixture)
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

        private UnitDetailLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UnitDetailLookupRepository(conn);
        }

        private async Task<(int CompanyId, int UnitId)> SeedPrereqsAsync(string unitName = "UD Unit")
        {
            await using var ctx = CreateDbContext();

            var company = new Company
            {
                CompanyName = "UD Co", LegalName = "UD Pvt", GstNumber = "GSTUD",
                TIN = "T", TAN = "T", CSTNo = "C", YearOfEstablishment = 2020,
                Website = "https://x.com", Logo = "l", EntityId = 1, PanNumber = "PANUD",
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                CompanyAddress = new CompanyAddress { AddressLine1 = "X", AddressLine2 = "Y", PinCode = "600001", AlternatePhone = "1", Phone = "2", CountryId = 1, StateId = 1, CityId = 1 },
                CompanyContact = new CompanyContact { Name = "X", Designation = "X", Email = "x@x.com", Phone = "1", Remarks = "X" }
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();

            var division = new UserManagement.Domain.Entities.Division
            {
                ShortName = "UD", Name = "UD Div", CompanyId = company.Id,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Divisions.AddAsync(division);
            await ctx.SaveChangesAsync();

            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "UNITTYPE");
            if (miscType == null)
            {
                miscType = new UserManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UNITTYPE", Description = "Unit Type",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var miscMaster = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "PLANT");
            if (miscMaster == null)
            {
                miscMaster = new UserManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = "PLANT", Description = "Plant", SortOrder = 1,
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(miscMaster);
                await ctx.SaveChangesAsync();
            }

            var unit = new UserManagement.Domain.Entities.Unit
            {
                UnitName = unitName, ShortName = "UD", CompanyId = company.Id, DivisionId = division.Id,
                UnitHeadName = "H", CINNO = "CIN123", OldUnitId = "OLD", IsMaintenanceStopStart = false,
                SpindlesCapacity = 100, UnitTypeId = miscMaster.Id,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                UnitAddress = new UnitAddress
                { CountryId = 1, StateId = 1, CityId = 5, AddressLine1 = "Addr 1", AddressLine2 = "Addr 2",
                  PinCode = 600042, ContactNumber = "1234567890", AlternateNumber = "0987654321" },
                UnitContacts = new UnitContacts
                { Name = "X", Designation = "X", Email = "x@x.com", PhoneNo = "1", Remarks = "X" }
            };
            await ctx.Unit.AddAsync(unit);
            await ctx.SaveChangesAsync();

            return (company.Id, unit.Id);
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Detail()
        {
            await ClearAsync();
            var (_, unitId) = await SeedPrereqsAsync("Detail Unit");

            var result = await CreateLookupRepo().GetByIdAsync(unitId);

            result.Should().NotBeNull();
            result!.UnitId.Should().Be(unitId);
            result.UnitName.Should().Be("Detail Unit");
            result.CINNO.Should().Be("CIN123");
            result.AddressLine1.Should().Be("Addr 1");
            result.PinCode.Should().Be(600042);
            result.Phone.Should().Be("1234567890");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateLookupRepo().GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var (_, unitId) = await SeedPrereqsAsync();

            await using var ctx = CreateDbContext();
            var unit = await ctx.Unit.FirstAsync(u => u.Id == unitId);
            unit.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(unitId);

            result.Should().BeNull();
        }
    }
}
