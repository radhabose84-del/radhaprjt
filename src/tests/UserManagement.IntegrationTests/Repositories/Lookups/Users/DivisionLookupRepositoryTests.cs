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
    public sealed class DivisionLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DivisionLookupRepositoryTests(DbFixture fixture)
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

        private DivisionLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DivisionLookupRepository(conn);
        }

        private async Task<int> EnsureCompanyAsync(string name = "DivLookup Co")
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyName == name && c.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var company = new Company
            {
                CompanyName = name,
                LegalName = $"{name} Pvt",
                GstNumber = "GSTDL",
                TIN = "TIN",
                TAN = "TAN",
                CSTNo = "CST",
                YearOfEstablishment = 2020,
                Website = "https://example.com",
                Logo = "logo.png",
                EntityId = 1,
                PanNumber = "PANDL",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted,
                CompanyAddress = new CompanyAddress
                {
                    AddressLine1 = "X", AddressLine2 = "Y", PinCode = "600001",
                    AlternatePhone = "1", Phone = "2", CountryId = 1, StateId = 1, CityId = 1
                },
                CompanyContact = new CompanyContact
                {
                    Name = "X", Designation = "X", Email = "x@x.com", Phone = "1", Remarks = "X"
                }
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();
            return company.Id;
        }

        private async Task<int> SeedDivisionAsync(int companyId, string shortName = "LKPDIV", string name = "Lookup Div")
        {
            await using var ctx = CreateDbContext();
            var division = new UserManagement.Domain.Entities.Division
            {
                ShortName = shortName,
                Name = name,
                CompanyId = companyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Divisions.AddAsync(division);
            await ctx.SaveChangesAsync();
            return division.Id;
        }

        private async Task<int> SeedUnitAsync(int companyId, int divisionId, string unitName = "Lookup Unit")
        {
            await using var ctx = CreateDbContext();

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
                UnitName = unitName, ShortName = "LU", CompanyId = companyId, DivisionId = divisionId,
                UnitHeadName = "H", CINNO = "CIN", OldUnitId = "OLD", IsMaintenanceStopStart = false,
                SpindlesCapacity = 100, UnitTypeId = miscMaster.Id,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                UnitAddress = new UnitAddress
                {
                    CountryId = 1, StateId = 1, CityId = 1, AddressLine1 = "A",
                    AddressLine2 = "B", PinCode = 600001, ContactNumber = "1", AlternateNumber = "2"
                },
                UnitContacts = new UnitContacts
                {
                    Name = "X", Designation = "X", Email = "x@x.com", PhoneNo = "1", Remarks = "X"
                }
            };
            await ctx.Unit.AddAsync(unit);
            await ctx.SaveChangesAsync();
            return unit.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllDivisionAsync ---

        [Fact]
        public async Task GetAllDivisionAsync_Should_Return_Seeded_Division()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            await SeedDivisionAsync(companyId, "LKPDIV", "Lookup Div");

            var results = await CreateLookupRepo().GetAllDivisionAsync();

            results.Should().Contain(d => d.Name == "Lookup Div");
        }

        [Fact]
        public async Task GetAllDivisionAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var id = await SeedDivisionAsync(companyId, "INACTIVE", "Inactive Div");

            await using var ctx = CreateDbContext();
            var div = await ctx.Divisions.FirstAsync(d => d.Id == id);
            div.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllDivisionAsync();

            results.Should().NotContain(d => d.Name == "Inactive Div");
        }

        [Fact]
        public async Task GetAllDivisionAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var id = await SeedDivisionAsync(companyId, "DEL", "Del Div");

            await using var ctx = CreateDbContext();
            var div = await ctx.Divisions.FirstAsync(d => d.Id == id);
            div.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllDivisionAsync();

            results.Should().NotContain(d => d.Name == "Del Div");
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Divisions()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var id1 = await SeedDivisionAsync(companyId, "D1", "Div 1");
            var id2 = await SeedDivisionAsync(companyId, "D2", "Div 2");
            await SeedDivisionAsync(companyId, "D3", "Div 3");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var id1 = await SeedDivisionAsync(companyId, "X", "X Div");
            var id2 = await SeedDivisionAsync(companyId, "Y", "Y Div");

            await using var ctx = CreateDbContext();
            var div = await ctx.Divisions.FirstAsync(d => d.Id == id1);
            div.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(1);
            results[0].Id.Should().Be(id2);
        }

        // --- GetUnitsByDivisionAsync ---

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Return_Matching_Units()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId, "UDIV", "Unit Div");
            await SeedUnitAsync(companyId, divId, "Unit A");
            await SeedUnitAsync(companyId, divId, "Unit B");

            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(companyId, divId);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Return_Empty_For_Invalid_Ids()
        {
            var results1 = await CreateLookupRepo().GetUnitsByDivisionAsync(0, 1);
            var results2 = await CreateLookupRepo().GetUnitsByDivisionAsync(1, 0);

            results1.Should().BeEmpty();
            results2.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Filter_By_CompanyId_And_DivisionId()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divA = await SeedDivisionAsync(companyId, "DA", "Div A");
            var divB = await SeedDivisionAsync(companyId, "DB", "Div B");
            await SeedUnitAsync(companyId, divA, "Unit A");
            await SeedUnitAsync(companyId, divB, "Unit B");

            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(companyId, divA);

            results.Should().HaveCount(1);
            results[0].UnitName.Should().Be("Unit A");
        }
    }
}
