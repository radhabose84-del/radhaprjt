using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Units;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Unit
{
    [Collection("DatabaseCollection")]
    public sealed class UnitCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UnitCommandRepositoryTests(DbFixture fixture)
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

        private UnitCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new UnitCommandRepository(ctx);

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

            var miscType = new UserManagement.Domain.Entities.MiscTypeMaster
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

            var miscMaster = new UserManagement.Domain.Entities.MiscMaster
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

        private UserManagement.Domain.Entities.Unit BuildUnit(
            int companyId,
            int divisionId,
            int unitTypeId,
            string unitName = "Test Unit",
            string shortName = "TU")
        {
            return new UserManagement.Domain.Entities.Unit
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
                IsActive = Enums.Status.Active,
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
        }

        private async Task ClearUnitsAsync(ApplicationDbContext ctx)
        {
            // Clear UnitAddress and UnitContacts first (FK dependency)
            var addresses = await ctx.UnitAddress.ToListAsync();
            ctx.UnitAddress.RemoveRange(addresses);

            var contacts = await ctx.UnitContacts.ToListAsync();
            ctx.UnitContacts.RemoveRange(contacts);

            await ctx.SaveChangesAsync();

            var units = await ctx.Unit.ToListAsync();
            ctx.Unit.RemoveRange(units);
            await ctx.SaveChangesAsync();
        }

        private async Task<(int CompanyId, int DivisionId, int UnitTypeId)> EnsurePrerequisitesAsync(ApplicationDbContext ctx)
        {
            var companyId = await EnsureCompanyAsync(ctx);
            var divisionId = await EnsureDivisionAsync(ctx, companyId);
            var miscTypeId = await EnsureMiscTypeMasterAsync(ctx);
            var unitTypeId = await EnsureMiscMasterAsync(ctx, miscTypeId);
            return (companyId, divisionId, unitTypeId);
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateUnitAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var unit = BuildUnit(companyId, divisionId, unitTypeId);

            var newId = await repo.CreateUnitAsync(unit);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateUnitAsync_Should_Persist_Unit_And_Nested_Entities()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var unit = BuildUnit(companyId, divisionId, unitTypeId, "Factory Unit", "FU");

            var newId = await repo.CreateUnitAsync(unit);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Unit
                .Include(u => u.UnitAddress)
                .Include(u => u.UnitContacts)
                .FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.UnitName.Should().Be("Factory Unit");
            saved.ShortName.Should().Be("FU");
            saved.CompanyId.Should().Be(companyId);
            saved.DivisionId.Should().Be(divisionId);
            saved.UnitHeadName.Should().Be("Test Head");
            saved.CINNO.Should().Be("CIN123456");
            saved.OldUnitId.Should().Be("OLD001");
            saved.IsMaintenanceStopStart.Should().BeFalse();
            saved.SpindlesCapacity.Should().Be(100);
            saved.UnitTypeId.Should().Be(unitTypeId);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);

            saved.UnitAddress.Should().NotBeNull();
            saved.UnitAddress!.AddressLine1.Should().Be("123 Test Street");
            saved.UnitAddress.PinCode.Should().Be(600001);
            saved.UnitAddress.ContactNumber.Should().Be("9876543210");

            saved.UnitContacts.Should().NotBeNull();
            saved.UnitContacts!.Name.Should().Be("Unit Contact");
            saved.UnitContacts.Email.Should().Be("unit@test.com");
        }

        [Fact]
        public async Task CreateUnitAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var unit = BuildUnit(companyId, divisionId, unitTypeId);

            var newId = await repo.CreateUnitAsync(unit);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Unit.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
            saved.CreatedByName.Should().NotBeNullOrEmpty();
            saved.CreatedIP.Should().NotBeNullOrEmpty();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateUnitAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var unit = BuildUnit(companyId, divisionId, unitTypeId, "Original Unit", "OU");
            var createdId = await repo.CreateUnitAsync(unit);
            ctx.ChangeTracker.Clear();

            var updateUnit = BuildUnit(companyId, divisionId, unitTypeId, "Updated Unit", "UU");
            updateUnit.UnitHeadName = "Updated Head";
            updateUnit.UnitAddress!.AddressLine1 = "Updated Address";
            updateUnit.UnitContacts!.Name = "Updated Contact";

            await using var ctx2 = CreateDbContext();
            var repo2 = CreateRepository(ctx2);
            var result = await repo2.UpdateUnitAsync(createdId, updateUnit);

            result.Should().Be(createdId);

            await using var ctx3 = CreateDbContext();
            var saved = await ctx3.Unit
                .Include(u => u.UnitAddress)
                .Include(u => u.UnitContacts)
                .FirstOrDefaultAsync(x => x.Id == createdId);

            saved.Should().NotBeNull();
            saved!.UnitName.Should().Be("Updated Unit");
            saved.ShortName.Should().Be("UU");
            saved.UnitHeadName.Should().Be("Updated Head");
            saved.UnitAddress!.AddressLine1.Should().Be("Updated Address");
            saved.UnitContacts!.Name.Should().Be("Updated Contact");
        }

        [Fact]
        public async Task UpdateUnitAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var unit = BuildUnit(companyId, divisionId, unitTypeId);

            var result = await repo.UpdateUnitAsync(99999, unit);

            result.Should().Be(-1);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteUnitAsync_Should_Soft_Delete_Unit()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var unit = BuildUnit(companyId, divisionId, unitTypeId);
            var createdId = await repo.CreateUnitAsync(unit);
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserManagement.Domain.Entities.Unit
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteUnitAsync(createdId, deleteModel);

            result.Should().Be(createdId);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.Unit.FirstOrDefaultAsync(x => x.Id == createdId);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteUnitAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var deleteModel = new UserManagement.Domain.Entities.Unit
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteUnitAsync(99999, deleteModel);

            result.Should().Be(-1);
        }

        // --- ExistsByCodeAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_For_Existing_UnitName()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateUnitAsync(BuildUnit(companyId, divisionId, unitTypeId, "Unique Unit", "UQ"));

            var exists = await repo.ExistsByCodeAsync("Unique Unit");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_For_NonExisting()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var exists = await repo.ExistsByCodeAsync("NonExistentUnit_XYZ");

            exists.Should().BeFalse();
        }

        // --- ExistsByNameupdateAsync ---

        [Fact]
        public async Task ExistsByNameupdateAsync_Should_Return_True_For_Duplicate_Name_Different_Id()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateUnitAsync(BuildUnit(companyId, divisionId, unitTypeId, "Dup Unit", "D1"));
            var secondId = await repo.CreateUnitAsync(BuildUnit(companyId, divisionId, unitTypeId, "Other Unit", "D2"));

            var exists = await repo.ExistsByNameupdateAsync("Dup Unit", secondId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameupdateAsync_Should_Return_False_For_Same_Id()
        {
            await using var ctx = CreateDbContext();
            await ClearUnitsAsync(ctx);
            var (companyId, divisionId, unitTypeId) = await EnsurePrerequisitesAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateUnitAsync(BuildUnit(companyId, divisionId, unitTypeId, "Same Unit", "SM"));

            var exists = await repo.ExistsByNameupdateAsync("Same Unit", id);

            exists.Should().BeFalse();
        }
    }
}
