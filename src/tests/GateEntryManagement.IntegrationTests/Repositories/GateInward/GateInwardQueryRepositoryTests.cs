using Microsoft.Data.SqlClient;
using Dapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Infrastructure.Repositories.GateInward;
using GateEntryManagement.Infrastructure.Data;
using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.IntegrationTests.Repositories.GateInward
{
    [Collection("DatabaseCollection")]
    public sealed class GateInwardQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GateInwardQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GateInwardQueryRepository CreateQueryRepo(
            Mock<IUnitLookup> unitLookup = null,
            Mock<IIPAddressService> ipService = null)
        {
            unitLookup ??= BuildDefaultUnitLookup();
            ipService ??= BuildDefaultIpService();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new GateInwardQueryRepository(conn, unitLookup.Object, ipService.Object);
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
            await _fixture.ClearAllTablesAsync();

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

        private async Task<int> SeedGateInwardAsync(ApplicationDbContext ctx, int vmrId,
            string gateEntryNo = "GE00001")
        {
            var entity = new GateEntryManagement.Domain.Entities.GateInwardHdr
            {
                GateEntryNo = gateEntryNo,
                VehicleMovementRecordId = vmrId,
                GrossWeight = 1000,
                TareWeight = 200,
                NetWeight = 800,
                QAInspectionRequired = false,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.GateInwardHdr.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task SoftDeleteGateInwardAsync(int id)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mockDocSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            var repo = new GateInwardCommandRepository(ctx, mockDocSeq.Object);
            await repo.SoftDeleteAsync(id, CancellationToken.None);
        }

        private async Task SeedDocumentPathMiscAsync(ApplicationDbContext ctx)
        {
            await ctx.MiscTypeMaster.AddRangeAsync(
                new GateEntryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = MiscEnumEntity.ImagePath,
                    Description = "http://192.168.1.126/Resources/",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                },
                new GateEntryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = MiscEnumEntity.GateEntryImage,
                    Description = "GateEntry",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> SeedGateInwardWithAttachmentAsync(ApplicationDbContext ctx, int vmrId)
        {
            var entity = new GateEntryManagement.Domain.Entities.GateInwardHdr
            {
                GateEntryNo = "GE-ATT-1",
                VehicleMovementRecordId = vmrId,
                GrossWeight = 1000,
                TareWeight = 200,
                NetWeight = 800,
                QAInspectionRequired = false,
                UnitId = 1,
                AttachmentFileName = "abc.pdf",
                AttachmentFilePath = "GateEntry/abc.pdf",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.GateInwardHdr.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- ATTACHMENT / DOCUMENT PATH ---

        [Fact]
        public async Task GetDocumentDirectoryPath_Should_Return_Both_Misc_Descriptions()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);
            await SeedDocumentPathMiscAsync(ctx);

            var dirs = await CreateQueryRepo().GetDocumentDirectoryPath();

            dirs[MiscEnumEntity.ImagePath].Should().Be("http://192.168.1.126/Resources/");
            dirs[MiscEnumEntity.GateEntryImage].Should().Be("GateEntry");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Compose_Attachment_Preview_Url()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            await SeedDocumentPathMiscAsync(ctx);
            var id = await SeedGateInwardWithAttachmentAsync(ctx, vmrId);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.AttachmentFileName.Should().Be("abc.pdf");
            dto.AttachmentFilePath.Should().Be("http://192.168.1.126/Resources/GateEntry/abc.pdf");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            await SeedGateInwardAsync(ctx, vmrId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await SoftDeleteGateInwardAsync(id);

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
            var id = await SeedGateInwardAsync(ctx, vmrId, "GE00001");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.GateEntryNo.Should().Be("GE00001");
            dto.VehicleMovementRecordId.Should().Be(vmrId);
            dto.GrossWeight.Should().Be(1000);
            dto.TareWeight.Should().Be(200);
            dto.NetWeight.Should().Be(800);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await SoftDeleteGateInwardAsync(id);

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
            var id = await SeedGateInwardAsync(ctx, vmrId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardAsync(ctx, vmrId, "GE00001");
            ctx.ChangeTracker.Clear();

            // Set inactive
            var entity = await ctx.GateInwardHdr.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("GE00001", CancellationToken.None);

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

        // --- MISC MASTER EXISTS ---

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, _) = await SeedPrerequisitesAsync(ctx);

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(miscMasterId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(9999);

            exists.Should().BeFalse();
        }
    }
}
