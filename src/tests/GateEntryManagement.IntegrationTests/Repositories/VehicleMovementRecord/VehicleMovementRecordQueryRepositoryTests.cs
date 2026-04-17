using Microsoft.Data.SqlClient;
using Dapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using GateEntryManagement.Infrastructure.Repositories.VehicleMovementRecord;
using GateEntryManagement.Infrastructure.Repositories.MiscTypeMaster;
using GateEntryManagement.Infrastructure.Repositories.MiscMaster;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.VehicleMovementRecord
{
    [Collection("DatabaseCollection")]
    public sealed class VehicleMovementRecordQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public VehicleMovementRecordQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private VehicleMovementRecordQueryRepository CreateQueryRepo(
            Mock<IPartyLookup> partyLookup = null,
            Mock<IUnitLookup> unitLookup = null,
            Mock<IIPAddressService> ipService = null)
        {
            partyLookup ??= BuildDefaultPartyLookup();
            unitLookup ??= BuildDefaultUnitLookup();
            ipService ??= BuildDefaultIpService();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new VehicleMovementRecordQueryRepository(
                conn, partyLookup.Object, unitLookup.Object, ipService.Object);
        }

        private Mock<IPartyLookup> BuildDefaultPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>());
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto)null);
            return mock;
        }

        private Mock<IUnitLookup> BuildDefaultUnitLookup()
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Test Unit" }
                });
            mock.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 1, UnitName = "Test Unit" });
            return mock;
        }

        private Mock<IIPAddressService> BuildDefaultIpService()
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(s => s.GetUnitId()).Returns(1);
            mock.Setup(s => s.GetCompanyId()).Returns(1);
            return mock;
        }

        private async Task<(int miscTypeId, int miscId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var typeId = await typeRepo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VMRSTATUS",
                Description = "VMR Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var miscId = await miscRepo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "IN",
                Description = "Inside Premises",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            return (typeId, miscId);
        }

        private async Task<int> SeedVMRAsync(ApplicationDbContext ctx, int miscMasterId,
            string vehicleMovementId = "VMR00001",
            string vehicleNumber = "KA01AB1234",
            string driverName = "John Doe",
            Status isActive = Status.Active)
        {
            var vmr = new GateEntryManagement.Domain.Entities.VehicleMovementRecord
            {
                VehicleMovementId = vehicleMovementId,
                VehicleNumber = vehicleNumber,
                DriverName = driverName,
                DriverMobileNo = "9876543210",
                PurposeOfVisitId = miscMasterId,
                StatusId = miscMasterId,
                GateInTime = DateTimeOffset.UtcNow,
                GateInBy = "test-user",
                UnitId = 1,
                IsActive = isActive,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.VehicleMovementRecord.AddAsync(vmr);
            await ctx.SaveChangesAsync();
            return vmr.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            // Soft delete the record
            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.IsDeleted = IsDelete.Deleted;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Alpha Driver");
            await SeedVMRAsync(ctx, miscId, "VMR00002", "MH02CD5678", "Beta Driver");
            ctx.ChangeTracker.Clear();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].DriverName.Should().Be("Alpha Driver");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Pagination_Metadata()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Driver A");
            await SeedVMRAsync(ctx, miscId, "VMR00002", "MH02CD5678", "Driver B");
            await SeedVMRAsync(ctx, miscId, "VMR00003", "TN03EF9012", "Driver C");
            ctx.ChangeTracker.Clear();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_StatusName_From_MiscMaster()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].StatusName.Should().Be("Inside Premises");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Test Driver");
            ctx.ChangeTracker.Clear();

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto.VehicleMovementId.Should().Be("VMR00001");
            dto.VehicleNumber.Should().Be("KA01AB1234");
            dto.DriverName.Should().Be("Test Driver");
            dto.StatusName.Should().Be("Inside Premises");
            dto.PurposeOfVisitName.Should().Be("Inside Premises");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.IsDeleted = IsDelete.Deleted;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);
            ctx.ChangeTracker.Clear();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.IsDeleted = IsDelete.Deleted;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Alpha Driver");
            await SeedVMRAsync(ctx, miscId, "VMR00002", "MH02CD5678", "Beta Driver");
            ctx.ChangeTracker.Clear();

            var results = await CreateQueryRepo().AutocompleteAsync("Alpha", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].DriverName.Should().Be("Alpha Driver");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Active Driver", Status.Active);
            await SeedVMRAsync(ctx, miscId, "VMR00002", "MH02CD5678", "Inactive Driver", Status.Inactive);
            ctx.ChangeTracker.Clear();

            var results = await CreateQueryRepo().AutocompleteAsync("Driver", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].DriverName.Should().Be("Active Driver");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Deleted Driver");
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.IsDeleted = IsDelete.Deleted;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- HAS OPEN VMR FOR VEHICLE ---

        [Fact]
        public async Task HasOpenVMRForVehicleAsync_Should_Return_True_For_Active_VMR()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Driver");
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().HasOpenVMRForVehicleAsync("KA01AB1234");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasOpenVMRForVehicleAsync_Should_Return_False_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().HasOpenVMRForVehicleAsync("ZZZZ999999");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasOpenVMRForVehicleAsync_Should_Return_False_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId, "VMR00001", "KA01AB1234", "Driver");
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.IsDeleted = IsDelete.Deleted;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().HasOpenVMRForVehicleAsync("KA01AB1234");

            result.Should().BeFalse();
        }

        // --- MISC MASTER EXISTS ---

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_Active()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().MiscMasterExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().MiscMasterExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);

            var misc = await ctx.MiscMaster.FirstAsync(x => x.Id == miscId);
            misc.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().MiscMasterExistsAsync(miscId);

            result.Should().BeFalse();
        }
    }
}
