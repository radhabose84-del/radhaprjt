using Contracts.Interfaces;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class CompanyDetailLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanyDetailLookupRepositoryTests(DbFixture fixture)
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

        private CompanyDetailLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CompanyDetailLookupRepository(conn);
        }

        private async Task<(int CompanyId, int UnitId)> SeedCompanyAndUnitAsync(
            string companyName = "Detail Co",
            string unitName = "Detail Unit")
        {
            await using var ctx = CreateDbContext();

            var company = new Company
            {
                CompanyName = companyName,
                LegalName = $"{companyName} Pvt",
                GstNumber = "GSTCD",
                TIN = "TIN",
                TAN = "TAN",
                CSTNo = "CST",
                YearOfEstablishment = 2020,
                Website = "https://example.com",
                Logo = "logo.png",
                EntityId = 1,
                PanNumber = "PANCD",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted,
                CompanyAddress = new CompanyAddress
                {
                    AddressLine1 = "Addr 1",
                    AddressLine2 = "Addr 2",
                    PinCode = "600001",
                    AlternatePhone = "9876543210",
                    Phone = "0123456789",
                    CountryId = 1,
                    StateId = 1,
                    CityId = 1
                },
                CompanyContact = new CompanyContact
                {
                    Name = "Co Contact",
                    Designation = "Manager",
                    Email = "co@example.com",
                    Phone = "9999999999",
                    Remarks = "Primary"
                }
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();

            var division = new UserManagement.Domain.Entities.Division
            {
                ShortName = "DDIV",
                Name = "Detail Division",
                CompanyId = company.Id,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Divisions.AddAsync(division);
            await ctx.SaveChangesAsync();

            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "UNITTYPE");
            if (miscType == null)
            {
                miscType = new UserManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UNITTYPE",
                    Description = "Unit Type",
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var miscMaster = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "PLANT");
            if (miscMaster == null)
            {
                miscMaster = new UserManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id,
                    Code = "PLANT",
                    Description = "Plant",
                    SortOrder = 1,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(miscMaster);
                await ctx.SaveChangesAsync();
            }

            var unit = new UserManagement.Domain.Entities.Unit
            {
                UnitName = unitName,
                ShortName = "DU",
                CompanyId = company.Id,
                DivisionId = division.Id,
                UnitHeadName = "Head",
                CINNO = "CIN123",
                OldUnitId = "OLD001",
                IsMaintenanceStopStart = false,
                SpindlesCapacity = 100,
                UnitTypeId = miscMaster.Id,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted,
                UnitAddress = new UnitAddress
                {
                    CountryId = 1,
                    StateId = 1,
                    CityId = 1,
                    AddressLine1 = "Unit Line",
                    AddressLine2 = "Unit Line 2",
                    PinCode = 600001,
                    ContactNumber = "9876543210",
                    AlternateNumber = "9876543211"
                },
                UnitContacts = new UnitContacts
                {
                    Name = "Unit Contact",
                    Designation = "Manager",
                    Email = "unit@example.com",
                    PhoneNo = "9876543212",
                    Remarks = "Test"
                }
            };
            await ctx.Unit.AddAsync(unit);
            await ctx.SaveChangesAsync();

            return (company.Id, unit.Id);
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetByUnitIdAsync ---

        [Fact]
        public async Task GetByUnitIdAsync_Should_Return_Matching_Details()
        {
            await ClearAsync();
            var (_, unitId) = await SeedCompanyAndUnitAsync("HappyCo", "HappyUnit");

            var result = await CreateLookupRepo().GetByUnitIdAsync(unitId);

            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("HappyCo");
            result.LegalName.Should().Be("HappyCo Pvt");
            result.GstNumber.Should().Be("GSTCD");
            result.PanNumber.Should().Be("PANCD");
            result.AddressLine1.Should().Be("Addr 1");
            result.PinCode.Should().Be("600001");
            result.Phone.Should().Be("0123456789");
            result.Email.Should().Be("co@example.com");
        }

        [Fact]
        public async Task GetByUnitIdAsync_Should_Return_Null_For_Unknown_Unit()
        {
            await ClearAsync();

            var result = await CreateLookupRepo().GetByUnitIdAsync(999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUnitIdAsync_Should_Return_Null_When_Unit_SoftDeleted()
        {
            await ClearAsync();
            var (_, unitId) = await SeedCompanyAndUnitAsync();

            await using var ctx = CreateDbContext();
            var unit = await ctx.Unit.FirstAsync(u => u.Id == unitId);
            unit.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByUnitIdAsync(unitId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUnitIdAsync_Should_Return_Null_When_Company_SoftDeleted()
        {
            await ClearAsync();
            var (companyId, unitId) = await SeedCompanyAndUnitAsync();

            await using var ctx = CreateDbContext();
            var company = await ctx.Companies.FirstAsync(c => c.Id == companyId);
            company.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByUnitIdAsync(unitId);

            result.Should().BeNull();
        }
    }
}
