using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.FinanceManagement;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.SalesManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.PartyMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.PartyMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterQueryRepository CreateRepo(
            Mock<IPartyMasterSalesValidation>? sales = null,
            Mock<IPartyMasterPurchaseValidation>? purchase = null,
            Mock<IPartyMasterFinanceValidation>? finance = null,
            Mock<IPartyMasterMaintenanceValidation>? maintenance = null,
            Mock<IDataAccessFilter>? dataAccess = null)
        {
            if (sales == null)
            {
                sales = new Mock<IPartyMasterSalesValidation>(MockBehavior.Loose);
                sales.Setup(s => s.HasLinkedPartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
                sales.Setup(s => s.HasActivePartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (purchase == null)
            {
                purchase = new Mock<IPartyMasterPurchaseValidation>(MockBehavior.Loose);
                purchase.Setup(p => p.HasLinkedPartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
                purchase.Setup(p => p.HasActivePartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (finance == null)
            {
                finance = new Mock<IPartyMasterFinanceValidation>(MockBehavior.Loose);
                finance.Setup(f => f.HasLinkedPartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
                finance.Setup(f => f.HasActivePartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (maintenance == null)
            {
                maintenance = new Mock<IPartyMasterMaintenanceValidation>(MockBehavior.Loose);
                maintenance.Setup(m => m.HasLinkedPartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
                maintenance.Setup(m => m.HasActivePartyMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (dataAccess == null)
            {
                dataAccess = new Mock<IDataAccessFilter>(MockBehavior.Loose);
                dataAccess.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DataAccessContext.Unrestricted);
            }

            var incoterm = new Mock<IIncotermLookup>(MockBehavior.Loose);
            var paymentTerm = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
            var city = new Mock<ICityLookup>(MockBehavior.Loose);
            var state = new Mock<IStateLookup>(MockBehavior.Loose);
            var country = new Mock<ICountryLookup>(MockBehavior.Loose);
            var salesSegment = new Mock<ISalesSegmentLookup>(MockBehavior.Loose);
            var freight = new Mock<IFreightMasterLookup>(MockBehavior.Loose);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyMasterQueryRepository(conn, _fixture.IpMock.Object,
                incoterm.Object, paymentTerm.Object, city.Object, state.Object, country.Object,
                dataAccess.Object, salesSegment.Object,
                sales.Object, purchase.Object, finance.Object, maintenance.Object,
                freight.Object);
        }

        // Seeds an ApprovalStatus type + Pending row + RegistrationType + Registered row.
        // Returns the registration MiscMaster id (to use as RegistrationTypeId).
        private async Task<int> EnsureMiscSeedAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var approval = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "ApprovalStatus");
            if (approval == null)
            {
                approval = new PartyManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "Approval",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(approval);
                await ctx.SaveChangesAsync();
            }
            if (!await ctx.MiscMaster.AnyAsync(m => m.Code == "Pending"))
            {
                await ctx.MiscMaster.AddAsync(new PartyManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = approval.Id, Code = "Pending", Description = "Pending", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
            }

            var regType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "RegistrationType");
            if (regType == null)
            {
                regType = new PartyManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "RegistrationType", Description = "Reg",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(regType);
                await ctx.SaveChangesAsync();
            }
            var registered = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "Registered" && m.MiscTypeId == regType.Id);
            if (registered == null)
            {
                registered = new PartyManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = regType.Id, Code = "Registered", Description = "Registered", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(registered);
                await ctx.SaveChangesAsync();
            }
            return registered.Id;
        }

        private async Task<int> SeedPartyAsync(string code, string name)
        {
            var regId = await EnsureMiscSeedAsync();
            var pendingId = await ctxPending();
            await using var ctx = _fixture.CreateFreshDbContext();
            var p = new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = code, PartyName = name,
                RegistrationTypeId = regId,
                StatusId = pendingId,
                UnitId = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                CreatedByName = "test-user", CreatedIP = "127.0.0.1"
            };
            await ctx.PartyMaster.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task<int> ctxPending()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await ctx.MiscMaster.Where(m => m.Code == "Pending").Select(m => m.Id).FirstAsync();
        }

        private async Task ClearPartyAsync() => await _fixture.ClearAllTablesAsync();

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing_Id()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing_Id()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PNF1", "NotFound1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children_And_No_CrossModule_Links()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PSDV1", "Sdv1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_PartyContact_Exists()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PSDV2", "Sdv2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await ctx.PartyContact.AddAsync(new PartyManagement.Domain.Entities.PartyContact
                {
                    PartyId = id,
                    FirstName = "First",
                    LastName = "Last",
                    ContactBy = "Primary",
                    EmailID = "x@y.com",
                    MobileNo = "1234567890"
                });
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_SalesValidation_Links()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PSDV3", "Sdv3");
            var sales = new Mock<IPartyMasterSalesValidation>(MockBehavior.Loose);
            sales.Setup(s => s.HasLinkedPartyMasterAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(sales: sales).SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        // --- IsPartyMasterLinkedAsync ---

        [Fact]
        public async Task IsPartyMasterLinkedAsync_Should_Return_False_When_No_Links()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PLK1", "Lk1");

            var result = await CreateRepo().IsPartyMasterLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsPartyMasterLinkedAsync_Should_Return_True_When_PurchaseValidation_Active()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PLK2", "Lk2");
            var purchase = new Mock<IPartyMasterPurchaseValidation>(MockBehavior.Loose);
            purchase.Setup(p => p.HasActivePartyMasterAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(purchase: purchase).IsPartyMasterLinkedAsync(id);

            result.Should().BeTrue();
        }

        // --- TransportDetailDuplicateExistsAsync ---

        [Fact]
        public async Task TransportDetailDuplicateExistsAsync_Should_Return_False_When_None_Match()
        {
            var result = await CreateRepo().TransportDetailDuplicateExistsAsync(1, 1, "NO_SUCH_VEHICLE");
            result.Should().BeFalse();
        }

        // --- GetCompanyUnitMapAsync ---

        [Fact]
        public async Task GetCompanyUnitMapAsync_Should_Return_Empty_When_No_Mappings()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PCU1", "CompanyUnit1");

            var (companyIds, unitIds) = await CreateRepo().GetCompanyUnitMapAsync(id);

            companyIds.Should().BeEmpty();
            unitIds.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCompanyUnitMapAsync_Should_Return_Distinct_Mappings()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PCU2", "CompanyUnit2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.PartyUnitCompanyMapping.AddRange(
                    new PartyManagement.Domain.Entities.PartyUnitCompanyMapping { PartyId = id, CompanyId = 1, UnitId = 1 },
                    new PartyManagement.Domain.Entities.PartyUnitCompanyMapping { PartyId = id, CompanyId = 1, UnitId = 2 },
                    new PartyManagement.Domain.Entities.PartyUnitCompanyMapping { PartyId = id, CompanyId = 2, UnitId = 1 });
                await ctx.SaveChangesAsync();
            }

            var (companyIds, unitIds) = await CreateRepo().GetCompanyUnitMapAsync(id);

            companyIds.Should().BeEquivalentTo(new[] { 1, 2 });
            unitIds.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        // --- GetPartyTypeCodesAsync ---

        [Fact]
        public async Task GetPartyTypeCodesAsync_Should_Return_Empty_When_None()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PTC1", "TypeCodes1");

            var result = await CreateRepo().GetPartyTypeCodesAsync(id);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPartyTypeCodesAsync_Should_Return_Uppercase_Codes()
        {
            await ClearPartyAsync();
            var id = await SeedPartyAsync("PTC2", "TypeCodes2");

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                // Seed a PartyType under the existing RegistrationType MiscMaster row
                var miscId = await ctx.MiscMaster.Where(m => m.Code == "Registered").Select(m => m.Id).FirstAsync();

                // PartyType requires a PartyGroup with GroupTypeId = miscId (FK).
                var pg = new PartyManagement.Domain.Entities.PartyGroup
                {
                    PartyGroupName = "TPG1",
                    GroupTypeId = miscId,
                    GlCategoryId = miscId, // FK satisfied by reusing same misc row
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.PartyGroup.AddAsync(pg);
                await ctx.SaveChangesAsync();

                await ctx.PartyType.AddAsync(new PartyManagement.Domain.Entities.PartyType
                {
                    PartyId = id,
                    PartyTypeId = miscId,
                    PartyGroupId = pg.Id
                });
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().GetPartyTypeCodesAsync(id);

            result.Should().Contain("REGISTERED");
        }

        // --- GetRegistrationDetails ---

        [Fact]
        public async Task GetRegistrationDetails_Should_Return_Matching_Row()
        {
            var regId = await EnsureMiscSeedAsync();

            var result = await CreateRepo().GetRegistrationDetails(regId);

            result.Should().NotBeNull();
            result.Description.Should().Be("Registered");
        }

        [Fact]
        public async Task GetRegistrationDetails_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetRegistrationDetails(9999999);
            result.Should().BeNull();
        }
    }
}
