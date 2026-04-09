using Microsoft.Data.SqlClient;
using Dapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Dtos.Lookups.Users;
using GateEntryManagement.Infrastructure.Repositories.GatePass;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.GatePass
{
    [Collection("DatabaseCollection")]
    public sealed class GatePassQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GatePassQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GatePassQueryRepository CreateQueryRepo(
            Mock<IUnitLookup> unitLookup = null,
            Mock<IIPAddressService> ipService = null)
        {
            unitLookup ??= BuildDefaultUnitLookup();
            ipService ??= BuildDefaultIpService();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new GatePassQueryRepository(conn, unitLookup.Object, ipService.Object);
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup(int unitId = 1, string unitName = "Test Unit")
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = unitId, UnitName = unitName }
                });
            mock.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = unitId, UnitName = unitName });
            return mock;
        }

        private static Mock<IIPAddressService> BuildDefaultIpService()
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(s => s.GetUnitId()).Returns(1);
            mock.Setup(s => s.GetCompanyId()).Returns(1);
            return mock;
        }

        private async Task<(int miscMasterId, int vmrId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
                DELETE FROM Gate.GateInwardDtl; DELETE FROM Gate.GateInwardHdr;
                DELETE FROM Gate.GatePassDtl; DELETE FROM Gate.GatePassHdr;
                DELETE FROM Gate.VehicleMovementRecord;
                DELETE FROM Gate.MiscMaster; DELETE FROM Gate.MiscTypeMaster;");

            var miscType = new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VMRSTATUS",
                Description = "VMR Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(miscType);
            await ctx.SaveChangesAsync();

            var miscMaster = new GateEntryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "INSIDE",
                Description = "Inside Premises",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(miscMaster);
            await ctx.SaveChangesAsync();

            var vmr = new GateEntryManagement.Domain.Entities.VehicleMovementRecord
            {
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobileNo = "9876543210",
                PurposeOfVisitId = miscMaster.Id,
                StatusId = miscMaster.Id,
                GateInTime = DateTimeOffset.UtcNow,
                GateInBy = "test-user",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.VehicleMovementRecord.AddAsync(vmr);
            await ctx.SaveChangesAsync();

            return (miscMaster.Id, vmr.Id);
        }

        private async Task<int> SeedGatePassAsync(ApplicationDbContext ctx, int vmrId,
            string gatePassNo = "GP00001")
        {
            var entity = new GateEntryManagement.Domain.Entities.GatePassHdr
            {
                GatePassNo = gatePassNo,
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleMovementRecordId = vmrId,
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = 1,
                TotalItems = 5,
                TotalDocumentQty = 100,
                TotalDispatchQty = 100,
                TotalValue = 50000,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.GatePassHdr.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task SoftDeleteGatePassAsync(int id, int vmrInStatusId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mockDocSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            var mockDocProcessors = new List<IGatePassDocumentProcessor>();
            var mockIpService = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            var repo = new GatePassCommandRepository(ctx, mockDocSeq.Object, mockDocProcessors, mockIpService.Object);
            await repo.SoftDeleteAsync(id, vmrInStatusId, CancellationToken.None);
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            await SeedGatePassAsync(ctx, vmrId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await SoftDeleteGatePassAsync(id, miscMasterId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId, "GP00001");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.GatePassNo.Should().Be("GP00001");
            dto.VehicleMovementRecordId.Should().Be(vmrId);
            dto.VehicleNumber.Should().Be("KA01AB1234");
            dto.DriverName.Should().Be("John Doe");
            dto.TotalItems.Should().Be(5);
            dto.TotalValue.Should().Be(50000);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await SoftDeleteGatePassAsync(id, miscMasterId);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId, "GP00001");
            ctx.ChangeTracker.Clear();

            // Set inactive
            var entity = await ctx.GatePassHdr.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("GP00001", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- VMR EXISTS ---

        [Fact]
        public async Task VehicleMovementRecordExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);

            var exists = await CreateQueryRepo().VehicleMovementRecordExistsAsync(vmrId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task VehicleMovementRecordExistsAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);

            var exists = await CreateQueryRepo().VehicleMovementRecordExistsAsync(9999);

            exists.Should().BeFalse();
        }
    }
}
