using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Units;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Unit
{
    [Collection("DatabaseCollection")]
    public sealed class UnitQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UnitQueryRepositoryTests(DbFixture fixture)
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

        private UnitQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UnitQueryRepository(conn, ipMock.Object);
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

        private async Task<int> EnsureDivisionAsync(ApplicationDbContext ctx, int companyId)
        {
            var existing = await ctx.Divisions.FirstOrDefaultAsync(d => d.CompanyId == companyId);
            if (existing != null)
                return existing.Id;

            var division = new UserManagement.Domain.Entities.Division
            {
                ShortName = "UTDIV",
                Name = "Unit Test Division",
                CompanyId = companyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Divisions.AddAsync(division);
            await ctx.SaveChangesAsync();
            return division.Id;
        }

        private async Task<int> EnsureMiscTypeMasterAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "UNITTYPE");
            if (existing != null)
                return existing.Id;

            var miscType = new MiscTypeMaster
            {
                MiscTypeCode = "UNITTYPE",
                Description = "Unit Type",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(miscType);
            await ctx.SaveChangesAsync();
            return miscType.Id;
        }

        private async Task<int> EnsureMiscMasterAsync(ApplicationDbContext ctx, int miscTypeId)
        {
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "PLANT");
            if (existing != null)
                return existing.Id;

            var miscMaster = new MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = "PLANT",
                Description = "Plant",
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(miscMaster);
            await ctx.SaveChangesAsync();
            return miscMaster.Id;
        }

        private async Task<(int CompanyId, int DivisionId, int UnitTypeId)> EnsurePrerequisitesAsync(ApplicationDbContext ctx)
        {
            var companyId = await EnsureCompanyAsync(ctx);
            var divisionId = await EnsureDivisionAsync(ctx, companyId);
            var miscTypeId = await EnsureMiscTypeMasterAsync(ctx);
            var unitTypeId = await EnsureMiscMasterAsync(ctx, miscTypeId);
            return (companyId, divisionId, unitTypeId);
        }

        private async Task<int> SeedUnitAsync(
            ApplicationDbContext ctx,
            int companyId,
            int divisionId,
            int unitTypeId,
            string unitName = "Test Unit",
            string shortName = "TU",
            Enums.Status isActive = Enums.Status.Active)
        {
            var unit = new UserManagement.Domain.Entities.Unit
            {
                UnitName = unitName,
                ShortName = shortName,
                CompanyId = companyId,
                DivisionId = divisionId,
                UnitHeadName = "Test Head",
                CINNO = "CIN123456",
                OldUnitId = "OLD001",
                IsMaintenanceStopStart = false,
                SpindlesCapacity = 100,
                UnitTypeId = unitTypeId,
                IsActive = isActive,
                IsDeleted = Enums.IsDelete.NotDeleted,
                UnitAddress = new UnitAddress
                {
                    CountryId = 1,
                    StateId = 1,
                    CityId = 1,
                    AddressLine1 = "123 Test Street",
                    AddressLine2 = "Suite 100",
                    PinCode = 600001,
                    ContactNumber = "9876543210",
                    AlternateNumber = "9876543211"
                },
                UnitContacts = new UnitContacts
                {
                    Name = "Unit Contact",
                    Designation = "Unit Manager",
                    Email = "unit@test.com",
                    PhoneNo = "9876543212",
                    Remarks = "Test contact"
                }
            };

            var repo = new UnitCommandRepository(ctx);
            var id = await repo.CreateUnitAsync(unit);
            ctx.ChangeTracker.Clear();
            return id;
        }

        private async Task ClearUnitsAsync(ApplicationDbContext ctx)
        {
            var addresses = await ctx.UnitAddress.ToListAsync();
            ctx.UnitAddress.RemoveRange(addresses);

            var contacts = await ctx.UnitContacts.ToListAsync();
            ctx.UnitContacts.RemoveRange(contacts);

            await ctx.SaveChangesAsync();

            var units = await ctx.Unit.ToListAsync();
            ctx.Unit.RemoveRange(units);
            await ctx.SaveChangesAsync();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllUnitsAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Alpha Unit", "AU");
            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Beta Unit", "BU");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllUnitsAsync(1, 10, null);

            items.Should().HaveCount(2);
            total.Should().Be(2);
        }

        [Fact]
        public async Task GetAllUnitsAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Alpha Unit", "AU");
            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Beta Unit", "BU");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllUnitsAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].UnitName.Should().Be("Alpha Unit");
        }

        [Fact]
        public async Task GetAllUnitsAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var id = await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Deleted Unit", "DU");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new UnitCommandRepository(ctx2);
            await cmdRepo.DeleteUnitAsync(id, new UserManagement.Domain.Entities.Unit
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllUnitsAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllUnitsAsync_Should_Include_UnitTypeName_From_MiscMaster()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Typed Unit", "TY");

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllUnitsAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].UnitTypeName.Should().Be("Plant");
        }

        [Fact]
        public async Task GetAllUnitsAsync_Should_Support_Pagination()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Unit One", "U1");
            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Unit Two", "U2");
            await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Unit Three", "U3");

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllUnitsAsync(1, 2, null);

            page1.Should().HaveCount(2);
            total.Should().Be(3);

            var (page2, _) = await repo.GetAllUnitsAsync(2, 2, null);
            page2.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Unit_With_Address_And_Contacts()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var id = await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "ById Unit", "BI");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.UnitName.Should().Be("ById Unit");
            result.ShortName.Should().Be("BI");
            result.UnitAddressDto.Should().NotBeNull();
            result.UnitAddressDto!.AddressLine1.Should().Be("123 Test Street");
            result.UnitContactsDto.Should().NotBeNull();
            result.UnitContactsDto!.Name.Should().Be("Unit Contact");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var id = await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Deleted Unit", "DU");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new UnitCommandRepository(ctx2);
            await cmdRepo.DeleteUnitAsync(id, new UserManagement.Domain.Entities.Unit
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo();
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

        // --- FK COLUMN EXIST VALIDATION ---

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Unit()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var id = await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Active Unit", "AC", Enums.Status.Active);

            var repo = CreateQueryRepo();
            var result = await repo.FKColumnExistValidation(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_Inactive_Unit()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var id = await SeedUnitAsync(ctx, companyId, divisionId, unitTypeId, "Inactive Unit", "IN", Enums.Status.Inactive);

            var repo = CreateQueryRepo();
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

        // --- MISC MASTER EXISTS ---

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_Active_MiscMaster()
        {
            await using var ctx = CreateDbContext();
            var (_, _, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateQueryRepo();
            var result = await repo.MiscMasterExistsAsync(unitTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.MiscMasterExistsAsync(99999);

            result.Should().BeFalse();
        }
    }
}
