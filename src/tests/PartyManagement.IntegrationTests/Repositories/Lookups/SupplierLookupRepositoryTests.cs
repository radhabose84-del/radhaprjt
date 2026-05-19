using Microsoft.Data.SqlClient;
using PartyManagement.Infrastructure.Repositories.Lookups;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class SupplierLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SupplierLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SupplierLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SupplierLookupRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedSupplierMiscAsync(string code = "Supplier")
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var miscType = new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "PartyType",
                Description = "Party Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = code,
                Description = code,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedPartyGroupAsync(int miscId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new PartyManagement.Domain.Entities.PartyGroup
            {
                PartyGroupName = "Default",
                GroupTypeId = miscId,
                GlCategoryId = miscId,
                IsGroup = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.PartyGroup.Add(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        // DbFixture mocks IIPAddressService.GetUnitId() => 1, so the lookup scopes to
        // unit 1. mappedUnitId controls which unit the supplier is mapped to.
        private async Task<int> SeedPartyAsync(
            int partyTypeMiscId,
            int groupId,
            string code,
            string name,
            Status active = Status.Active,
            int mappedUnitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var party = new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = code,
                PartyName = name,
                RegistrationTypeId = partyTypeMiscId,
                StatusId = partyTypeMiscId,
                UnitId = 1,
                IsActive = active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.PartyMaster.Add(party);
            await ctx.SaveChangesAsync();

            var partyType = new PartyManagement.Domain.Entities.PartyType
            {
                PartyId = party.Id,
                PartyTypeId = partyTypeMiscId,
                PartyGroupId = groupId
            };
            ctx.PartyType.Add(partyType);

            ctx.PartyUnitCompanyMapping.Add(new PartyManagement.Domain.Entities.PartyUnitCompanyMapping
            {
                PartyId = party.Id,
                CompanyId = 1,
                UnitId = mappedUnitId
            });
            await ctx.SaveChangesAsync();

            return party.Id;
        }

        [Fact]
        public async Task SearchSuppliers_NoData_ReturnsEmpty()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().SearchSuppliersAsync("anything");

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchSuppliers_ReturnsActiveSupplier_ByName()
        {
            await _fixture.ClearAllTablesAsync();
            var supplierMiscId = await SeedSupplierMiscAsync("Supplier");
            var groupId = await SeedPartyGroupAsync(supplierMiscId);
            await SeedPartyAsync(supplierMiscId, groupId, "SUP001", "Acme Industrial Supplies");

            var result = await CreateRepo().SearchSuppliersAsync("Acme");

            result.Should().ContainSingle();
            result[0].VendorCode.Should().Be("SUP001");
            result[0].VendorName.Should().Be("Acme Industrial Supplies");
        }

        [Fact]
        public async Task SearchSuppliers_ReturnsActiveSupplier_ByCode()
        {
            await _fixture.ClearAllTablesAsync();
            var supplierMiscId = await SeedSupplierMiscAsync("Supplier");
            var groupId = await SeedPartyGroupAsync(supplierMiscId);
            await SeedPartyAsync(supplierMiscId, groupId, "SUP777", "Beta Traders");

            var result = await CreateRepo().SearchSuppliersAsync("SUP777");

            result.Should().ContainSingle();
            result[0].VendorName.Should().Be("Beta Traders");
        }

        [Fact]
        public async Task SearchSuppliers_ExcludesInactiveParty()
        {
            await _fixture.ClearAllTablesAsync();
            var supplierMiscId = await SeedSupplierMiscAsync("Supplier");
            var groupId = await SeedPartyGroupAsync(supplierMiscId);
            await SeedPartyAsync(supplierMiscId, groupId, "SUP002", "Inactive Vendor", Status.Inactive);

            var result = await CreateRepo().SearchSuppliersAsync("Inactive");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchSuppliers_ExcludesSupplierMappedToDifferentUnit()
        {
            await _fixture.ClearAllTablesAsync();
            var supplierMiscId = await SeedSupplierMiscAsync("Supplier");
            var groupId = await SeedPartyGroupAsync(supplierMiscId);
            // Caller is unit 1 (fixture mock); this supplier is mapped to unit 2.
            await SeedPartyAsync(supplierMiscId, groupId, "SUP900", "Other Unit Vendor",
                Status.Active, mappedUnitId: 2);

            var result = await CreateRepo().SearchSuppliersAsync("Other Unit");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActiveSupplierById_ReturnsNull_WhenMappedToDifferentUnit()
        {
            await _fixture.ClearAllTablesAsync();
            var supplierMiscId = await SeedSupplierMiscAsync("Supplier");
            var groupId = await SeedPartyGroupAsync(supplierMiscId);
            var partyId = await SeedPartyAsync(supplierMiscId, groupId, "SUP901", "Cross Unit",
                Status.Active, mappedUnitId: 2);

            var result = await CreateRepo().GetActiveSupplierByIdAsync(partyId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveSupplierById_ReturnsSupplier_WhenActive()
        {
            await _fixture.ClearAllTablesAsync();
            var supplierMiscId = await SeedSupplierMiscAsync("Supplier");
            var groupId = await SeedPartyGroupAsync(supplierMiscId);
            var partyId = await SeedPartyAsync(supplierMiscId, groupId, "SUP010", "Gamma Supply Co");

            var result = await CreateRepo().GetActiveSupplierByIdAsync(partyId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(partyId);
            result.VendorName.Should().Be("Gamma Supply Co");
        }

        [Fact]
        public async Task GetActiveSupplierById_ReturnsNull_WhenNotSupplier()
        {
            await _fixture.ClearAllTablesAsync();
            var customerMiscId = await SeedSupplierMiscAsync("Customer");
            var groupId = await SeedPartyGroupAsync(customerMiscId);
            var partyId = await SeedPartyAsync(customerMiscId, groupId, "CUS001", "Some Customer");

            var result = await CreateRepo().GetActiveSupplierByIdAsync(partyId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveSupplierById_ReturnsNull_ForZeroId()
        {
            var result = await CreateRepo().GetActiveSupplierByIdAsync(0);
            result.Should().BeNull();
        }
    }
}
